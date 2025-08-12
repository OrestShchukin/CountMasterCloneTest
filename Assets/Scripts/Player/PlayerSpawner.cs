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
    [Header("Setup")] public GameObject followerPrefab;
    public Transform followerParent;
    public List<GameObject> followers = new();


    [Header("Formation Settings")] public float minDistance = 0.3f;
    public float radiusStep = 0.3f;
    public float regroupDelay = 0.4f;

    [Header("UI Settings")] public TextMeshPro textCounter;

    [Header("Finish Settings")] [SerializeField]
    GameObject followerStairsContainer;

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
        followers.RemoveAll(f => f == null);

        int count = followers.Count;
        float positionY = 0f;

        List<Vector3> positions =
            await Task.Run(() => GenerateRingBlueNoise(count, minDistance, radiusStep, positionY));

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
        int relaxIterations = 8;
        float cell = Mathf.Max(0.001f, minDistance);
        float targetRadius = Mathf.Sqrt(Mathf.Max(1, count)) * cell * 0.9f;
        float repelRadius = cell * 2.0f;
        float repelStrength = 1.0f;
        float centerPull = 0.35f;
        float jitter = cell * 0.05f;

        // ВАЖЛИВО: використовуємо System.Random (жодного UnityEngine.Random у Task.Run)
        var rng = new System.Random(unchecked(Environment.TickCount * 31 + count));

        var pts = PoissonWithRadialBias(count, cell, targetRadius, yPos, rng);
        Relax(pts, relaxIterations, repelRadius, repelStrength, centerPull, jitter, rng);

        pts.Sort((a, b) =>
        {
            float ra = a.sqrMagnitude;
            float rb = b.sqrMagnitude;
            if (ra != rb) return ra.CompareTo(rb);
            // другий ключ — кут
            float aa = Mathf.Atan2(a.z, a.x), ab = Mathf.Atan2(b.z, b.x);
            return aa.CompareTo(ab);
        });

        pts = StableRemapToPreviousSlots(pts);

        // привести y
        for (int i = 0; i < pts.Count; i++)
        {
            var p = pts[i];
            p.y = yPos;
            pts[i] = p;
        }

        return pts;

        // ---------- helpers ----------
        List<Vector3> PoissonWithRadialBias(int n, float rMin, float R, float y, System.Random rnd)
        {
            var list = new List<Vector3>(n);
            float cellSize = rMin / Mathf.Sqrt(2f);
            var grid = new Dictionary<(int, int), Vector3>();

            int maxTries = 20000;
            double kBias = 2.2;

            for (int tries = 0; tries < maxTries && list.Count < n; tries++)
            {
                double u = rnd.NextDouble();
                float r = R * Mathf.Sqrt((float)u);
                double ang = rnd.NextDouble() * Math.PI * 2.0;

                var p = new Vector3((float)Math.Cos(ang) * r, y, (float)Math.Sin(ang) * r);

                double pr = Math.Exp(-kBias * (r * r) / (R * R));
                if (rnd.NextDouble() > pr) continue;

                int gx = Mathf.RoundToInt(p.x / cellSize);
                int gz = Mathf.RoundToInt(p.z / cellSize);

                bool ok = true;
                for (int dx = -2; dx <= 2 && ok; dx++)
                for (int dz = -2; dz <= 2 && ok; dz++)
                {
                    var key = (gx + dx, gz + dz);
                    if (grid.TryGetValue(key, out var q))
                        if ((q - p).sqrMagnitude < rMin * rMin)
                            ok = false;
                }

                if (!ok) continue;

                list.Add(p);
                grid[(gx, gz)] = p;
            }

            // догенерація, якщо раптом не вистачило
            while (list.Count < n)
            {
                double ang = rnd.NextDouble() * Math.PI * 2.0;
                float r = R * Mathf.Sqrt((float)rnd.NextDouble());
                list.Add(new Vector3((float)Math.Cos(ang) * r, y, (float)Math.Sin(ang) * r));
            }

            return list;
        }

        void Relax(List<Vector3> pts, int iters, float influenceR, float repelK, float centerK, float jitt,
            System.Random rnd)
        {
            float ir2 = influenceR * influenceR;
            for (int it = 0; it < iters; it++)
            {
                float cell = influenceR * 0.7071f;
                var grid = new Dictionary<(int, int), List<int>>();
                for (int i = 0; i < pts.Count; i++)
                {
                    int gx = Mathf.RoundToInt(pts[i].x / cell);
                    int gz = Mathf.RoundToInt(pts[i].z / cell);
                    var key = (gx, gz);
                    if (!grid.TryGetValue(key, out var list)) grid[key] = list = new List<int>();
                    list.Add(i);
                }

                var delta = new Vector3[pts.Count];
                for (int i = 0; i < pts.Count; i++)
                {
                    var pi = pts[i];
                    int gx = Mathf.RoundToInt(pi.x / cell);
                    int gz = Mathf.RoundToInt(pi.z / cell);

                    Vector3 force = Vector3.zero;
                    for (int dx = -1; dx <= 1; dx++)
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        var key = (gx + dx, gz + dz);
                        if (!grid.TryGetValue(key, out var bucket)) continue;
                        for (int b = 0; b < bucket.Count; b++)
                        {
                            int j = bucket[b];
                            if (j == i) continue;
                            Vector3 d = pi - pts[j];
                            float d2 = d.sqrMagnitude;
                            if (d2 < ir2 && d2 > 1e-6f)
                            {
                                float inv = 1f / Mathf.Sqrt(d2);
                                force += d * inv * (repelK * 0.5f);
                            }
                        }
                    }

                    force += (-pi) * centerK;

                    // шум через System.Random
                    float nx = (float)(rnd.NextDouble() * 2.0 - 1.0);
                    float nz = (float)(rnd.NextDouble() * 2.0 - 1.0);
                    force.x += nx * jitt;
                    force.z += nz * jitt;

                    delta[i] = force;
                }

                float step = 0.25f / (1f + it * 0.3f);
                for (int i = 0; i < pts.Count; i++)
                    pts[i] += delta[i] * step;
            }
        }

        List<Vector3> StableRemapToPreviousSlots(List<Vector3> newPts)
        {
            if (s_prevSlots == null || s_prevSlots.Count == 0)
            {
                s_prevSlots = new List<Vector3>(newPts);
                return newPts;
            }

            int n = newPts.Count;
            var used = new bool[n];
            var mapped = new List<Vector3>(Mathf.Min(s_prevSlots.Count, n));

            for (int i = 0; i < Mathf.Min(s_prevSlots.Count, n); i++)
            {
                int best = -1;
                float bestD2 = float.MaxValue;
                for (int j = 0; j < n; j++)
                {
                    if (used[j]) continue;
                    float d2 = (s_prevSlots[i] - newPts[j]).sqrMagnitude;
                    if (d2 < bestD2)
                    {
                        bestD2 = d2;
                        best = j;
                    }
                }

                if (best < 0) best = Array.IndexOf(used, false);
                used[best] = true;
                mapped.Add(newPts[best]);
            }

            for (int j = 0; j < n; j++)
                if (!used[j])
                    mapped.Add(newPts[j]);

            s_prevSlots = new List<Vector3>(mapped);
            return mapped;
        }
    }

// Не забудь це поле всередині класу PlayerSpawner (ти вже додав):
    static List<Vector3> s_prevSlots;


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
                        transform.position.z + blockLength * i,
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
        sequence.Append(transform
            .DOMove(
                new Vector3(0f, (heightModifier) * characterHeight,
                    transform.position.z + heightModifier * blockLength), (heightModifier * blockLength) / forwardSpeed)
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
                for (int j = 0; j < staircase.transform.childCount; j++)
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
            Transform lastParentContainer =
                staircaseFollowerParentContainers[staircaseFollowerParentContainers.Count() - 1].transform;
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