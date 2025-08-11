using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using TMPro;
using Random = UnityEngine.Random;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Setup")]
    public GameObject followerPrefab;
    public Transform followerParent;
    public List<GameObject> followers = new();


    [Header("Formation Settings")] 
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
    private float pathDistance;

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
        if (waitingForRegroup)
        {
            RegroupOnDemand();
        }
    }

    public void AddFollowersOld(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 spawnPosition = transform.position;
            GameObject newFollower = Instantiate(followerPrefab, spawnPosition, Quaternion.identity, followerParent);
            followers.Add(newFollower);
        }

        FormatStickMan();
    }

    public void AddFollowers(int amount)
    {
        StartCoroutine(AddFollowersCoroutine(amount));
    }
    
    public void MultiplyFollowers(int factor)
    {
        int currentCount = followers.Count;
        int targetTotal = currentCount * factor;
        int toAdd = targetTotal - currentCount;

        AddFollowers(toAdd);
    }

    private IEnumerator AddFollowersCoroutine(int amount)
    {
        int batchsize = 30;

        for (int i = 0; i < amount; i++)
        {
            Vector3 spawnPosition = transform.position;
            GameObject newFollower = Instantiate(followerPrefab, spawnPosition, Quaternion.identity, followerParent);
            followers.Add(newFollower);

            if (i % batchsize == 0)
                yield return null;
        }

        _ = GenerateAndApplyPositionsAsync();
    }

    private async Task GenerateAndApplyPositionsAsync(bool transitionInstant = false)
    {
        followers.RemoveAll(f => f == null) ;
        
        int count = followers.Count;
        float positionY = 0f;
        
        List<Vector3> positions = await Task.Run(() => GenerateRingBlueNoise(count, minDistance, radiusStep, positionY));
        
        ApplyPositions(positions, transitionInstant);
    }

    private void ApplyPositions(List<Vector3> positions, bool transitionInstant = false)
    {
        int count = Mathf.Min(followerParent.childCount, positions.Count);

        for (int i = 0; i < count; i++)
        {
            Transform child = followerParent.GetChild(i);
            if (!transitionInstant)
                child.DOLocalMove(positions[i], 1f).SetEase(Ease.OutBack);
            else
                child.DOLocalMove(positions[i], 1f).SetEase(Ease.Linear);
            child.rotation = basicRotation;
        }
        
        textCounter.text = followers.Count.ToString();
    }





    public void FormatStickMan(bool transitionInstant = false)
    {
        _ = GenerateAndApplyPositionsAsync(transitionInstant);
    }


    public static List<Vector3> GenerateRingBlueNoise(int count, float minDistance, float radiusStep, float yPos)
    {
        var random = new System.Random();
        List<Vector3> positions = new List<Vector3>();
        float radius = radiusStep;
        int maxAttempts = 30;

        while (positions.Count < count)
        {
            int attempts = 0;

            while (attempts < maxAttempts && positions.Count < count)
            {
                double angle = random.NextDouble() * Mathf.PI * 2;
                double offset = (random.NextDouble() * 0.8 - 0.4) * radiusStep;

                float r = radius + (float)offset;

                Vector3 candidate = new Vector3(
                    Mathf.Cos((float)angle) * r,
                    yPos,
                    Mathf.Sin((float)angle) * r
                );

                bool valid = true;

                foreach (var pos in positions)
                {
                    if (Vector3.Distance(candidate, pos) < minDistance)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                    positions.Add(candidate);

                attempts++;
            }

            radius += radiusStep;
        }

        return positions;
    }

    public void StickmansBuildPyramid()
    {
        CameraSwitcher.cameraSwitcherInstance.ActivateCinemachineCamera(2);
        int maxLevelStickmans = 5;
        float yHeight = characterHeight, xWidth = characterWidth, currentHeight = 0f;
        float delayBetweenMoves = 0.1f, totalDelay = 0f;
        followers.RemoveAll(item => !item);
        int count = followers.Count;


        for (int i = 0; i < count;)
        {


            int numInRow = 0;

            if (i < count - 11)
                numInRow = maxLevelStickmans;
            else if (count - i >= 4)
                numInRow = 4;
            else if (count - i >= 3)
                numInRow = 3;
            else if (count - i >= 2)
                numInRow = 2;
            else
                numInRow = 1;

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
    }

    public void StickmansBuildStairs()
    {   
        int count = followerParent.childCount;
        List<GameObject> staircaseFollowerParentContainers = new List<GameObject>();
        Vector3 startPos = transform.position;
        GameObject endDestinationContainer = new GameObject("EndDestinationContainer");
        endDestinationContainer.transform.position = startPos;
        float blockLength = 2f;
        float forwardSpeed = PlayerControl.playerControlInstance.forwardSpeed;

        for (int i = 0; i < count; i++)
        {
            staircaseFollowerParentContainers.Add(followerParent.GetChild(i).gameObject);
        }
        
        for (int i = 0; i < staircaseFollowerParentContainers.Count; i++)
        {
            GameObject staircase = staircaseFollowerParentContainers[i];
            if (i < 12)
            {
                staircase.transform.SetParent(endDestinationContainer.transform);
                staircase.transform.DOMoveZ(
                        transform.position.z  + blockLength * i,
                        (i * blockLength) / forwardSpeed)
                    .SetEase(Ease.Linear)
                    .OnComplete(() => SetAnimationStand(staircase));
            }
            else
            {
                staircase.transform.SetParent(endDestinationContainer.transform);
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
            FormatStickMan(true);
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
    
    

    public void DestroyAndDelete(GameObject item)
    {
        followers.Remove(item);
        DOTween.Kill(item.transform);
        Destroy(item);
        textCounter.text = followers.Count.ToString();
        ScheduleRegroup();
    }


    public void ScheduleRegroup()
    {
        regroupTimer = regroupDelay;
        waitingForRegroup = true;
        lastFollowerCount = followers.Count;
    }

    void RegroupOnDemand()
    {
        regroupTimer -= Time.deltaTime;

        if (followers.Count != lastFollowerCount)
        {
            regroupTimer = regroupDelay;
            lastFollowerCount = followers.Count;
        }

        if (regroupTimer <= 0f)
        {
            waitingForRegroup = false;
            FormatStickMan();
        }
    }

    public void PauseRegroup()
    {
        waitingForRegroup = false;
    }
    // Debug functions

    public void ChangeNumOfStickMansDebug(int count)
    {
        int followersCount = followers.Count;
        if (followersCount < count)
        {
            AddFollowers(count - followersCount);
        }
        else
        {
            for (int i = 0, max = followersCount - count; i < max; i++)
            {
                DestroyAndDelete(followers[i]);
            }
        }
    }


    // Finish

    
    

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