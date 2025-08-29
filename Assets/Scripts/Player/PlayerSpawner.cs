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

    // ===== Engage / Surround =====

    [Header("Engage Settings")] public float enemyBaseRadius = 0.6f; // орієнтовний "розмір" ворога
    public float ringPaddingMul = 1.15f; // множник між кільцями від мін. дистанції
    public float arcPaddingMul = 1.10f; // множник по дузі (щоб не торкались сусіди)
    public float surroundTween = 0.6f; // час анімації до слоту

    bool isEngaging;
    Transform engagedEnemy;

    private bool waitingForRegroup = false;
    private int lastFollowerCount = 0;
    private Quaternion basicRotation;

    private float regroupTimer;
    private float pathDistance;
    
    // Boss Attack
    private float smallestEngageRadius;
    void Awake()
    {
        playerSpawnerInstance = this;
        followers.Clear();
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
                    new Vector3(transform.position.x, currentHeight, transform.position.z), Quaternion.identity,
                    followerParent);
                for (int j = 0; j < numOfPlayersInRow && (i + j) < followers.Count; j++)
                {
                    float xPos = xWidth * (j - (numOfPlayersInRow / 2) + 0.5f);
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

        HandleFollowerDeathInEngage(item);

        Destroy(item);
        if (AudioManager.instance)
        {
            AudioManager.instance.PlayForAmountOfTime("StickmanDeath", 0.05f);
        }
        textCounter.text = followers.Count.ToString();
        if (isEngaging) return;
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

    class Slot
    {
        public int ring;
        public float angle; // в радіанах
        public Vector3 worldPos;
        public GameObject occupant; // follower або null
    }


    readonly List<Slot> _slots = new List<Slot>();
    readonly Dictionary<GameObject, int> _followerToSlot = new Dictionary<GameObject, int>();

    public void EngageEnemy(Transform enemy)
    {
        if (enemy == null) return;
        engagedEnemy = enemy;
        isEngaging = true;

        followers.RemoveAll(f => f == null);

        BuildSlotsForCount(followers.Count, out var perRing, out var radii);
        
        AssignFollowersToSlots(perRing, radii);
        
        TweenFollowersToSlots();
    }
    public void DisengageEnemy()
    {
        isEngaging = false;
        engagedEnemy = null;
        _slots.Clear();
        _followerToSlot.Clear();
        // Далі можеш викликати свій FormatStickMan(), щоб повернутися до звичної формації
    }
    
    void HandleFollowerDeathInEngage(GameObject dead)
    {
        if (!isEngaging || dead == null) return;
        if (!_followerToSlot.TryGetValue(dead, out var slotIdx)) return;

        var vac = _slots[slotIdx];
        vac.occupant = null;
        _followerToSlot.Remove(dead);

        // Якщо це не перше кільце — НІЧОГО не робимо (позиції не змінюються)
        if (vac.ring != 0) return;

        // Знайти кандидата з другого кільця, чий кут найближче до vac.angle
        int bestIdx = -1;
        float bestDelta = float.MaxValue;

        for (int i = 0; i < _slots.Count; i++)
        {
            var s = _slots[i];
            if (s.ring != 1) continue; // лише друге кільце
            if (s.occupant == null) continue; // слот порожній
            float d = AngleDelta(s.angle, vac.angle); // різниця по куту
            if (d < bestDelta)
            {
                bestDelta = d;
                bestIdx = i;
            }
        }

        if (bestIdx >= 0)
        {
            // Пересадити кандидата у внутрішній слот
            var donor = _slots[bestIdx];
            var go = donor.occupant;
            donor.occupant = null;

            vac.occupant = go;
            _slots[slotIdx] = vac; // оновили

            if (go != null)
            {
                _followerToSlot[go] = slotIdx;
                MoveOneFollowerTo(go, vac.worldPos); // тільки цей рухається
                LookAtEnemy(go.transform);
            }
        }
    }

    // ---------------- helpers ----------------

    float AngleDelta(float a, float b)
    {
        // нормалізувати до [-pi, pi]
        float d = Mathf.Repeat(a - b + Mathf.PI, Mathf.PI * 2f) - Mathf.PI;
        return Mathf.Abs(d);
    }

    void BuildSlotsForCount(int totalFollowers, out List<int> perRing, out List<float> radii)
    {
        _slots.Clear();
        perRing = new List<int>();
        radii = new List<float>();
        if (engagedEnemy == null) return;

        // геометрія
        float cell = Mathf.Max(0.001f, minDistance);
        float ringSpacing = cell * ringPaddingMul; // між кільцями
        float arcSpacing = cell * arcPaddingMul; // мін. дугова відстань між сусідами
        float r0 = enemyBaseRadius + cell * 0.7f;

        // ПІВКОЛО: ширина дуги = 180° (можеш звузити/розширити)
        float halfWidth = Mathf.PI * 0.3f; // 90° в обидві сторони
        Vector3 approach = (transform.position - engagedEnemy.position);
        float centerAngle = Mathf.Atan2(approach.z, approach.x);

        int placed = 0;
        int ring = 0;

        while (placed < totalFollowers)
        {
            float radius = r0 + ring * ringSpacing;

            // місткість саме ПІВКОЛА за довжиною дуги: L = 2*halfWidth*radius
            int capArc = Mathf.Max(1, Mathf.FloorToInt((2f * halfWidth * radius) / arcSpacing));

            perRing.Add(capArc);
            radii.Add(radius);

            // рівномірно по дузі [centerAngle - halfWidth, centerAngle + halfWidth]
            for (int i = 0; i < capArc && placed < totalFollowers; i++)
            {
                float t = capArc == 1 ? 0.5f : i / (float)(capArc - 1);
                float ang = Mathf.Lerp(centerAngle - halfWidth, centerAngle + halfWidth, t);
                Vector3 pos = engagedEnemy.position + new Vector3(Mathf.Cos(ang) * radius, 0f, Mathf.Sin(ang) * radius);

                _slots.Add(new Slot { ring = ring, angle = ang, worldPos = pos, occupant = null });
                placed++;
            }

            ring++;
        }
        smallestEngageRadius = radii[0];
    }


    // Призначення фоловерів слотам по кільцях, без перетинів
    void AssignFollowersToSlots(List<int> perRing, List<float> radii)
    {
        _followerToSlot.Clear();
        if (engagedEnemy == null) return;

        // фоловерів сортуємо за відстанню (кільце) і кутом (щоб мінімізувати перетини)
        var items = new List<(GameObject go, float dist, float ang)>(followers.Count);
        foreach (var f in followers)
        {
            if (f == null) continue;
            Vector3 v = f.transform.position - engagedEnemy.position;
            items.Add((f, v.magnitude, Mathf.Atan2(v.z, v.x)));
        }

        items.Sort((a, b) =>
        {
            int d = a.dist.CompareTo(b.dist);
            return d != 0 ? d : a.ang.CompareTo(b.ang);
        });

        int slotStart = 0;
        int itemIndex = 0;

        for (int ring = 0; ring < perRing.Count; ring++)
        {
            int ringSlotCount = Mathf.Min(perRing[ring], _slots.Count - slotStart);
            if (ringSlotCount <= 0) break;

            int take = Mathf.Min(ringSlotCount, items.Count - itemIndex);
            if (take <= 0) break;

            // індекси слотів цього кільця
            var ringIndices = new List<int>(ringSlotCount);
            for (int si = 0; si < ringSlotCount; si++) ringIndices.Add(slotStart + si);
            ringIndices.Sort((i1, i2) => _slots[i1].angle.CompareTo(_slots[i2].angle));

            // беремо стільки фоловерів, скільки слотів у півколі цього кільця
            var ringFollowers = items.GetRange(itemIndex, take);
            ringFollowers.Sort((a, b) => a.ang.CompareTo(b.ang));
            itemIndex += take;

            for (int i = 0; i < take; i++)
            {
                var go = ringFollowers[i].go;
                int globalIdx = ringIndices[i];

                _slots[globalIdx].occupant = go;
                _followerToSlot[go] = globalIdx;
            }

            slotStart += ringSlotCount; 
        }
    }
    
    


    void TweenFollowersToSlots(float speed = 0)
    {
        foreach (var kv in _followerToSlot)
        {
            var go = kv.Key;
            var slot = _slots[kv.Value];
            MoveOneFollowerTo(go, slot.worldPos, speed);
            LookAtEnemy(engagedEnemy.transform);

            float radius = smallestEngageRadius == 0 ? smallestEngageRadius : enemyBaseRadius;
            if ((go.transform.position - engagedEnemy.position).magnitude < radius * 1.2f + ringPaddingMul)
            {
                BossScript.bossScriptInstance.decreasePerSecond++;
            }
        }
    }

    void MoveOneFollowerTo(GameObject go, Vector3 worldPos, float speed = 0)
    {
        speed = speed == 0 ? surroundTween : speed;
        if (go == null) return;
        DOTween.Kill(go.transform); // скасувати старі рухи
        Vector3 distance =  worldPos - go.transform.position;
        go.transform.DOMove(worldPos, speed * distance.magnitude).SetEase(Ease.OutQuad);
    }

    void LookAtEnemy(Transform t)
    {
        if (t == null || engagedEnemy == null) return;
        Vector3 look = engagedEnemy.position;
        look.y = t.position.y;
        t.LookAt(look);
    }

    public void OnEnemyHitReformat()
    {
        PauseRegroup(); // щоб авто-реґруп не боровся з нами

        if (isEngaging && engagedEnemy != null)
        {
            RebuildSurround(); 
        }
        else
        {
            FormatStickMan(true); 
        }
    }

// зручний реюз: те ж саме, що робить EngageEnemy, але без зміни прапорів
    public void RebuildSurround()
    {
        if (CalculateActualFollowersCount() == 0) return;
        BossScript.bossScriptInstance.decreasePerSecond = 0;
        BuildSlotsForCount(followers.Count, out var perRing, out var radii);
        AssignFollowersToSlots(perRing, radii);
        TweenFollowersToSlots(1.2f);
    }

    public int CalculateActualFollowersCount()
    {
        followers.RemoveAll(f => f == null);
        return followers.Count;
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