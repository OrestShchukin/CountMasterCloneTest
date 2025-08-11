using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using TMPro;
using Random = UnityEngine.Random;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Setup")]
    public GameObject followerPrefab;
    public Transform followerParent;
    public List<GameObject> followers = new();

    [Header("Formation (NavMesh crowd)")]
    public FormationManager formation; // assign in Inspector

    [Header("Fallback layout (legacy)")]
    public float minDistance = 0.3f;
    public float radiusStep = 0.3f;
    public float regroupDelay = 0.4f;
 
    [Header("UI Settings")]
    public TextMeshPro textCounter;
    
    [Header("Finish Settings")]
    [SerializeField] GameObject followerStairsContainer;

    [SerializeField] private float characterHeight = 1.5f, characterWidth = 1f;
    public static PlayerSpawner playerSpawnerInstance;
    
    private bool waitingForRegroup = false;
    private int lastFollowerCount = 0;
    private Quaternion basicRotation;

    private float regroupTimer;

    void Awake()
    {
        playerSpawnerInstance = this;
        followers.Add(followerParent.GetChild(0).gameObject);
        textCounter.text = followers.Count.ToString();
        basicRotation = followerParent.GetChild(0).rotation;
        DOTween.SetTweensCapacity(500, 350);
    }

    void Update()
    {
        if (waitingForRegroup) RegroupOnDemand();
    }

    public void AddFollowers(int amount) => StartCoroutine(AddFollowersCoroutine(amount));

    // Keep old API for gates: multiply current crowd size
    public void MultiplyFollowers(int factor)
    {
        if (factor <= 1) return; // no-op
        int current = followers.Count;
        int target = current * factor;
        int toAdd = Mathf.Max(0, target - current);
        if (toAdd > 0) AddFollowers(toAdd);
    }


    private IEnumerator AddFollowersCoroutine(int amount)
    {
        int batchsize = 30;
        for (int i = 0; i < amount; i++)
        {
            Vector3 spawnPosition = transform.position;
            GameObject newFollower = Instantiate(followerPrefab, spawnPosition, Quaternion.identity, followerParent);
            EnsureFollowerAgent(newFollower);
            followers.Add(newFollower);
            if (i % batchsize == 0) yield return null;
        }

        if (formation != null)
        {
            formation.RebindFollowersFrom(followers);
            formation.SetCenterToLeader();
            formation.RefreshNow();
            StickmansSetAnimRun();
        }
        else
        {
            _ = GenerateAndApplyPositionsAsync();
        }
    }

    private void EnsureFollowerAgent(GameObject go)
    {
        if (!go.GetComponent<FollowerAgent>()) go.AddComponent<FollowerAgent>();
    }

    private async Task GenerateAndApplyPositionsAsync(bool transitionInstant = false)
    {
        followers.RemoveAll(f => f == null);
        int count = followers.Count; float positionY = 0f;
        List<Vector3> positions = await Task.Run(() => GenerateRingBlueNoise(count, minDistance, radiusStep, positionY));
        ApplyPositions(positions, transitionInstant);
    }

    private void ApplyPositions(List<Vector3> positions, bool transitionInstant = false)
    {
        int count = Mathf.Min(followerParent.childCount, positions.Count);
        for (int i = 0; i < count; i++)
        {
            Transform child = followerParent.GetChild(i);
            if (!transitionInstant) child.DOLocalMove(positions[i], 1f).SetEase(Ease.OutBack);
            else child.DOLocalMove(positions[i], 1f).SetEase(Ease.Linear);
            child.rotation = basicRotation;
        }
        textCounter.text = followers.Count.ToString();
    }

    public static List<Vector3> GenerateRingBlueNoise(int count, float minDistance, float radiusStep, float yPos)
    {
        var random = new System.Random();
        List<Vector3> positions = new List<Vector3>();
        float radius = radiusStep; int maxAttempts = 30;
        while (positions.Count < count)
        {
            int attempts = 0;
            while (attempts < maxAttempts && positions.Count < count)
            {
                double angle = random.NextDouble() * Mathf.PI * 2;
                double offset = (random.NextDouble() * 0.8 - 0.4) * radiusStep;
                float r = radius + (float)offset;
                Vector3 candidate = new Vector3(Mathf.Cos((float)angle) * r, yPos, Mathf.Sin((float)angle) * r);
                bool valid = true;
                foreach (var pos in positions) if (Vector3.Distance(candidate, pos) < minDistance) { valid = false; break; }
                if (valid) positions.Add(candidate);
                attempts++;
            }
            radius += radiusStep;
        }
        return positions;
    }

    public void DestroyAndDelete(GameObject item)
    {
        followers.Remove(item);
        DOTween.Kill(item.transform);
        Destroy(item);
        textCounter.text = followers.Count.ToString();
        if (formation != null) formation.OnFollowerDied(item);
        ScheduleRegroup();
    }

    public void ScheduleRegroup()
    {
        regroupTimer = regroupDelay; waitingForRegroup = true; lastFollowerCount = followers.Count;
    }

    void RegroupOnDemand()
    {
        regroupTimer -= Time.deltaTime;
        if (followers.Count != lastFollowerCount) { regroupTimer = regroupDelay; lastFollowerCount = followers.Count; }
        if (regroupTimer <= 0f) { waitingForRegroup = false; FormatStickMan(); }
    }

    public void PauseRegroup() => waitingForRegroup = false;

    public void FormatStickMan(bool transitionInstant = false)
    {
        if (formation != null)
        {
            followers.RemoveAll(f => f == null);
            formation.RebindFollowersFrom(followers);
            formation.SetCenterToLeader();
            formation.RefreshNow();
            StickmansSetAnimRun();
            textCounter.text = followers.Count.ToString();
            return;
        }
        _ = GenerateAndApplyPositionsAsync(transitionInstant);
    }

    // === Helpers for cinematics ===
    void ToggleAgents(bool enabled)
    {
        foreach (var go in followers)
        {
            if (!go) continue;
            var agent = go.GetComponent<NavMeshAgent>();
            if (agent) agent.enabled = enabled;
        }
    }

    void RebindAfterCinematics()
    {
        // rebuild followers list from current children (in case parenting changed)
        followers.Clear();
        for (int i = 0; i < followerParent.childCount; i++)
        {
            var child = followerParent.GetChild(i).gameObject;
            if (child) followers.Add(child);
        }

        ToggleAgents(true);

        if (formation != null)
        {
            formation.RebindFollowersFrom(followers);
            formation.SetCenterToLeader();
            formation.Resume();
        }
        else
        {
            _ = GenerateAndApplyPositionsAsync(true);
        }

        textCounter.text = followers.Count.ToString();
    }

    // === Unique zones ===
    public void StickmansBuildPyramid()
    {
        // Cinematic mode: stop formation steering & disable agents
        formation?.Suspend();
        ToggleAgents(false);

        CameraSwitcher.cameraSwitcherInstance.ActivateCinemachineCamera(2);
        int maxLevelStickmans = 5;
        float yHeight = characterHeight, xWidth = characterWidth, currentHeight = 0f;
        float delayBetweenMoves = 0.1f, totalDelay = 0f;
        followers.RemoveAll(item => !item);
        int count = followers.Count;

        for (int i = 0; i < count;)
        {
            int numInRow = 0;

            if (i < count - 11) numInRow = maxLevelStickmans;
            else if (count - i >= 4) numInRow = 4;
            else if (count - i >= 3) numInRow = 3;
            else if (count - i >= 2) numInRow = 2;
            else numInRow = 1;

            SpawnLane(numInRow);

            void SpawnLane(int numOfPlayersInRow)
            {
                if (i >= followers.Count) return;
                GameObject levelParent = Instantiate(followerStairsContainer,
                    new Vector3(0f, currentHeight, transform.position.z), Quaternion.identity, followerParent);
                for (int j = 0; j < numOfPlayersInRow && (i + j) < followers.Count; j++)
                {
                    float xPos = xWidth * (j - (numOfPlayersInRow / 2));
                    xPos = numOfPlayersInRow % 2 != 0 ? xPos - 0.5f * xWidth : xPos;
                    followers[i + j].transform.DOLocalMove(new Vector3(xPos, 0f, 0f), 1f)
                        .SetEase(Ease.OutBack).SetDelay(totalDelay);
                    followers[i + j].transform.parent = levelParent.transform;
                }

                totalDelay += delayBetweenMoves;
                i += numOfPlayersInRow;
                currentHeight += yHeight;
            }
        }

        Transform highestContainer = followerParent.GetChild(followerParent.childCount - 1);
        CameraSwitcher.cameraSwitcherInstance.SwitchCameraTarget(2, highestContainer, 3f);
        // No immediate resume here; pyramid is a finish cinematic. Resume when you need to revert.
    }

    public void StickmansBuildStairs()
    {   
        // Cinematic mode: stop formation steering & disable agents
        formation?.Suspend();
        ToggleAgents(false);

        int count = followerParent.childCount;
        List<GameObject> staircaseFollowerParentContainers = new List<GameObject>();
        Vector3 startPos = transform.position;
        GameObject endDestinationContainer = new GameObject("EndDestinationContainer");
        endDestinationContainer.transform.position = startPos;
        float blockLength = 2f;
        float forwardSpeed = PlayerControl.playerControlInstance.forwardSpeed;

        for (int i = 0; i < count; i++)
            staircaseFollowerParentContainers.Add(followerParent.GetChild(i).gameObject);
        
        for (int i = 0; i < staircaseFollowerParentContainers.Count; i++)
        {
            GameObject staircase = staircaseFollowerParentContainers[i];
            staircase.transform.SetParent(endDestinationContainer.transform);

            if (i < 12)
            {
                staircase.transform.DOMoveZ(
                        transform.position.z  + blockLength * i,
                        (i * blockLength) / forwardSpeed)
                    .SetEase(Ease.Linear)
                    .OnComplete(() => SetAnimationStand(staircase));
            }
            else
            {
                staircase.transform.DOMoveZ(
                        transform.position.z + blockLength * 14,
                        (14 * blockLength) / forwardSpeed)
                    .SetEase(Ease.Linear);
            }
        }

        int reachedStepsCount = endDestinationContainer.transform.childCount;
        int heightModifier = reachedStepsCount <= 12 ? reachedStepsCount : 13;
        heightModifier--;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(new Vector3(0f, (heightModifier) * characterHeight, transform.position.z + heightModifier * blockLength), (heightModifier * blockLength) / forwardSpeed)
            .SetEase(Ease.Linear));

        if (reachedStepsCount < 13)
        {
            PlayerControl.allowMovement = false;
            sequence.Play().OnComplete(() => StartCoroutine(showWinScreenAftterDelay(1f)));
        }
        else
        {
            PlayerControl.allowMovement = false;
            sequence.Play().OnComplete(() => StartCoroutine(moveAfterDelay()));
        }
        
        void changeParentBack()
        {
            followers.Clear();
            for (int i = 12; i < staircaseFollowerParentContainers.Count; i++)
            {
                GameObject staircase = staircaseFollowerParentContainers[i];
                List<Transform> staircaseChildren = new List<Transform>();
                for ( int j = 0; j < staircase.transform.childCount; j++)
                {
                    Transform child = staircase.transform.GetChild(j);
                    staircaseChildren.Add(child);
                    followers.Add(child.gameObject);
                }
                foreach (Transform child in staircaseChildren)
                {
                    child.SetParent(followerParent);
                }
            }
        }
        
        IEnumerator moveAfterDelay()
        {
            changeParentBack();
            PlayerControl.allowMovement = true;
            yield return new WaitForSeconds(0.3f);
            RebindAfterCinematics();
        }

        IEnumerator showWinScreenAftterDelay(float duration)
        {
            CameraSwitcher.cameraSwitcherInstance.ActivateCinemachineCamera(5);
            yield return new WaitForSeconds(0.5f);
            Transform lastParentContainer = staircaseFollowerParentContainers[staircaseFollowerParentContainers.Count() - 1].transform;
            setChildStickmansAnimationDance();
            yield return new WaitForSeconds(duration);
            PlayerControl.playerControlInstance.PlayerWin();

            void setChildStickmansAnimationDance()
            {
                for (int i = 0; i < lastParentContainer.childCount; i++)
                {
                    Animator animator = lastParentContainer.GetChild(i).GetChild(0).GetComponent<Animator>();
                    animator.SetTrigger("WaveDance");
                    animator.ResetTrigger("Running");
                }
            }
        }
        
        void SetAnimationStand(GameObject staircase)
        {
            for (int i = 0; i < staircase.transform.childCount; i++)
            {
                Animator animator = staircase.transform.GetChild(i).GetChild(0).GetComponent<Animator>();
                animator.ResetTrigger("Running");
                animator.SetTrigger("DynIdle");
            }
        }
    }

    // Animations 
    public void StickmansSetAnimDance()
    {
        followers.RemoveAll(item => !item);
        for (int i = 0; i < followers.Count; i++)
        {
            Animator childAnimator = followerParent.GetChild(i).GetChild(0).GetComponent<Animator>();
            childAnimator.SetTrigger("WaveDance");
            childAnimator.ResetTrigger("Running");
        }
    }

    public void StickmansSetAnimRun()
    {
        followers.RemoveAll(item => item == null);
        for (int i = 0; i < followers.Count; i++)
        {
            Animator childAnimator = followerParent.GetChild(i).GetChild(0).GetComponent<Animator>();
            childAnimator.SetTrigger("Running");
        }
    }

    public void StickmansSetAnimStand()
    {
        followers.RemoveAll(item => !item);
        for (int i = 0; i < followers.Count; i++)
        {
            Animator childAnimator = followerParent.GetChild(i).GetChild(0).GetComponent<Animator>();
            childAnimator.SetTrigger("DynIdle");
        }
    }

    public void StickmansSetAnimJump()
    {
        followers.RemoveAll(item => !item);
        for (int i = 0; i < followers.Count; i++)
        {
            Animator childAnimator = followerParent.GetChild(i).GetChild(0).GetComponent<Animator>();
            childAnimator.SetTrigger("Jumping");
        }
    }
}