using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class PlayerSpawnerScriptBackUP : MonoBehaviour
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
    [SerializeField] TextMeshPro textCounter;
    
    [Header("Finish Settings")]
    [SerializeField] GameObject followerStairsContainer;
    
    public static PlayerSpawnerScriptBackUP playerSpawnerInstance;
    
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

    public void AddFollowers(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 spawnPosition = transform.position;
            GameObject newFollower = Instantiate(followerPrefab, spawnPosition, Quaternion.identity, followerParent);
            followers.Add(newFollower);
        }

        FormatStickMan();
    }


    public void MultiplyFollowers(int factor)
    {
        int currentCount = followers.Count;
        int targetTotal = currentCount * factor;
        int toAdd = targetTotal - currentCount;

        AddFollowers(toAdd);
    }


    public void FormatStickMan()
    {
        followers.RemoveAll(item => !item);
        int count = followers.Count;

        var positions = GenerateRingBlueNoise(count, minDistance, radiusStep);

        for (int i = 0; i < count; i++)
        {
            Transform child  = followerParent.GetChild(i);
            child.DOLocalMove(positions[i], 1f).SetEase(Ease.OutBack);
            child.rotation = basicRotation;
        }

        textCounter.text = followers.Count.ToString();
    }


    public static List<Vector3> GenerateRingBlueNoise(int count, float minDistance, float radiusStep)
    {
        List<Vector3> positions = new List<Vector3>();
        float radius = radiusStep;
        int maxAttempts = 30;

        while (positions.Count < count)
        {
            int attempts = 0;

            while (attempts < maxAttempts && positions.Count < count)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float randomOffset = Random.Range(-radiusStep * 0.4f, radiusStep * 0.4f);

                float r = radius; //  + randomOffset

                Vector3 candidate = new Vector3(
                    Mathf.Cos(angle) * r,
                    0f,
                    Mathf.Sin(angle) * r
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


    public void StickmansBuildStairs()
    {
        int maxLevelStickmans = 5;
        float yHeight = 1.5f, xWidth = 1f, currentHeight = 0f;
        float delayBetweenMoves = 0.1f, totalDelay = 0f;
        followers.RemoveAll(item => !item);
        int count = followers.Count;


        for (int i = 0; i < count;)
        {
            // if (i < count - 11)
            // {
            //     SpawnLane(maxLevelStickmans);
            //
            // }
            // else
            // {
            //     for (int k = 4; k > 0; k--)
            //     {
            //         SpawnLane(k);
            //     }
            // }

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
                    // pyramidSequence.Append(followers[i + j].transform.DOLocalMove(new Vector3(xPos, currentHeight, 0f), 6f)
                    //     .SetEase(Ease.OutBack).SetDelay(totalDelay));
                    followers[i + j].transform.DOLocalMove(new Vector3(xPos, 0f, 0f), 1f)
                        .SetEase(Ease.OutBack).SetDelay(totalDelay);
                    followers[i + j].transform.parent = levelParent.transform;
                }

                totalDelay += delayBetweenMoves;

                i += numOfPlayersInRow;
                currentHeight += yHeight;
            }
        }
    }

    // public void StickmansBuildStairs()
    // {
    //     int maxInRow = 5;
    //     float yHeight = 1.5f;
    //     float xWidth = 1f;
    //     float delayBetweenMoves = 0.2f;
    //
    //     followers.RemoveAll(item => item == null);
    //     int count = followers.Count;
    //
    //     List<int> rowLayout = GenerateTriangleLayout(count, maxInRow);
    //
    //     Sequence sequence = DOTween.Sequence();
    //     int index = 0;
    //
    //     for (int row = 0; row < rowLayout.Count; row++)
    //     {
    //         int rowCount = rowLayout[row];
    //         float yPos = row * yHeight;
    //
    //         // створюємо контейнер для рядка
    //         GameObject rowParent = new GameObject($"Row_{row}");
    //         rowParent.transform.SetParent(transform);
    //         rowParent.transform.localPosition = new Vector3(0, 0, 0);
    //
    //         for (int i = 0; i < rowCount && index < count; i++, index++)
    //         {
    //             float xPos = xWidth * (i - (rowCount * 0.5f));
    //             if (rowCount % 2 != 0) xPos -= xWidth * 0.5f;
    //
    //             var follower = followers[index].transform;
    //             follower.SetParent(rowParent.transform);
    //             Vector3 localPos = new Vector3(xPos, yPos, 0f);
    //             sequence.Append(follower.DOLocalMove(localPos, 0.5f).SetEase(Ease.OutBack).SetDelay(delayBetweenMoves));
    //         }
    //     }
    //
    //     sequence.Play();
    // }
    //
    //
    //  
    // private List<int> GenerateTriangleLayout(int totalFollowers, int maxInRow)
    // {
    //     List<int> layout = new List<int>();
    //     int remaining = totalFollowers;
    //
    //     // Пошук максимальної кількості рядків, щоб зверху був 1
    //     int rows = 0;
    //     int size = maxInRow;
    //     int testRemaining = remaining;
    //
    //     while (testRemaining > 0)
    //     {
    //         int current = Mathf.Min(size, testRemaining);
    //         testRemaining -= current;
    //         rows++;
    //         if (size > 1) size--;
    //     }
    //
    //     // Побудова розкладки з конкретними розмірами рядків
    //     size = maxInRow;
    //     for (int i = 0; i < rows; i++)
    //     {
    //         int current = Mathf.Min(size, remaining);
    //         layout.Add(current);
    //         remaining -= current;
    //         if (size > 1) size--;
    //     }
    //
    //     return layout;
    // }


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
        followers.RemoveAll(item => !item);
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