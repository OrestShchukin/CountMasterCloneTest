using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FormationManager : MonoBehaviour
{
    [Header("Centers")]
    public Transform leader;
    public Transform enemy;
    public bool aroundEnemy;

    [Header("Followers (wired from PlayerSpawner)")]
    public List<FollowerAgent> followers = new List<FollowerAgent>();

    [Header("Crowd layout (no rings)")]
    [Tooltip("Max number of virtual crowd slots (should cover max possible followers)")]
    public int capacity = 400;
    [Tooltip("Base spacing between neighbors in meters")]
    public float spacing = 0.35f;

    [Header("Navigation sampling / update")]
    public float sampleMaxDistance = 0.6f; // how far from desired we try to snap to NavMesh
    public float retargetHz = 8f;          // destination refresh rate

    [Header("Local relaxation (hole filling)")]
    public float neighborClaimAngleDeg = 20f; // who can claim an inner hole (angular window)
    public int maxClaimsPerTick = 4;          // limit moves per update to avoid ripple
    public bool useKinematicOffMesh = true;   // when desired point has no NavMesh under it

    [Header("Runtime control")]
    public bool suspended = false;            // when true, formation stops steering (for cinematics)

    class Slot
    {
        public int id;
        public Vector3 local;        // local offset relative to center (leader/enemy)
        public float angleDeg;       // for neighbor relation
        public float radius;         // sqrt-based radius
        public Vector3 worldDesired; // center.TransformPoint(local)
        public Vector3 worldNav;     // closest point on NavMesh (if found)
        public bool onNav;           // has NavMesh nearby
        public bool occupied;
        public FollowerAgent owner;
    }

    readonly List<Slot> slots = new();
    float timer;

    Transform CurrentCenter => aroundEnemy && enemy ? enemy : leader;

    const float GoldenAngleRad = 2.39996322972865332f; // ~137.507764° in radians

    // --- Public API ---
    public void RebindFollowersFrom(List<GameObject> goList)
    {
        EnsureSlotsCapacity(Mathf.Max(goList.Count, 1));

        // rebuild follower list & preserve existing assignments when possible
        followers.Clear();
        foreach (var go in goList)
        {
            if (!go) continue;
            var fa = go.GetComponent<FollowerAgent>();
            if (!fa) fa = go.AddComponent<FollowerAgent>();
            followers.Add(fa);
        }

        // clear slot ownership
        foreach (var s in slots) { s.owner = null; s.occupied = false; }

        // 1) try to reuse their previous slot ids
        foreach (var f in followers)
        {
            if (f.slotId >= 0 && f.slotId < slots.Count && !slots[f.slotId].occupied)
            {
                slots[f.slotId].occupied = true;
                slots[f.slotId].owner = f;
            }
            else f.slotId = -1; // will be assigned below
        }

        // 2) assign the rest to lowest free ids (keeps crowd compact, no reshuffle)
        int cursor = 0;
        foreach (var f in followers)
        {
            if (f.slotId >= 0) continue;
            while (cursor < slots.Count && slots[cursor].occupied) cursor++;
            if (cursor >= slots.Count) break; // out of capacity
            var s = slots[cursor];
            s.occupied = true; s.owner = f; f.BindSlot(s.id);
            cursor++;
        }

        // bind look target
        var center = CurrentCenter != null ? CurrentCenter : leader;
        foreach (var f in followers) f.BindLook(center);

        RefreshNow();
    }

    public void OnFollowerDied(GameObject go)
    {
        // free its slot & remove from list
        foreach (var s in slots)
        {
            if (s.owner && s.owner.gameObject == go)
            {
                s.owner = null; s.occupied = false;
                break;
            }
        }
        followers.RemoveAll(f => f == null || f.gameObject == go);
    }

    public void SetCenterToLeader()
    {
        aroundEnemy = false;
        foreach (var f in followers) f.BindLook(leader);
    }

    public void SetCenterToEnemy(Transform enemyCenter)
    {
        enemy = enemyCenter;
        aroundEnemy = true;
        foreach (var f in followers) f.BindLook(enemy);
    }

    public void Suspend() { suspended = true; }
    public void Resume() { suspended = false; RefreshNow(); }

    public void RefreshNow()
    {
        if (suspended) return;
        UpdateSlotPositions();
        LocalRelaxation();
        PushDestinations();
    }

    void Update()
    {
        if (suspended || CurrentCenter == null || followers.Count == 0) return;
        timer += Time.deltaTime;
        if (timer >= 1f / Mathf.Max(1f, retargetHz))
        {
            timer = 0f;
            UpdateSlotPositions();
            LocalRelaxation();
            PushDestinations();
        }
    }

    // --- Internals ---
    void EnsureSlotsCapacity(int need)
    {
        if (slots.Count >= Mathf.Min(capacity, need + 64)) return; // small headroom

        // (Re)generate up to capacity using phyllotaxis (Fermat spiral) — crowd-like, no rings
        slots.Clear();
        int cap = Mathf.Clamp(Mathf.Max(need + 64, capacity), 1, capacity);
        for (int i = 0; i < cap; i++)
        {
            float r = spacing * Mathf.Sqrt(i);
            float a = i * GoldenAngleRad;
            var local = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);

            slots.Add(new Slot
            {
                id = i,
                local = local,
                angleDeg = a * Mathf.Rad2Deg,
                radius = r,
                occupied = false,
                owner = null
            });
        }
    }

    void UpdateSlotPositions()
    {
        var center = CurrentCenter; if (!center) return;
        // compute desired & nav positions for first K slots where K≈max used id + headroom
        int maxUsedId = -1;
        for (int i = 0; i < slots.Count; i++) if (slots[i].occupied) maxUsedId = Mathf.Max(maxUsedId, i);
        int K = Mathf.Clamp(maxUsedId + 32, 0, slots.Count);

        for (int i = 0; i < K; i++)
        {
            var s = slots[i];
            s.worldDesired = center.TransformPoint(s.local);
            if (NavMesh.SamplePosition(s.worldDesired, out var hit, sampleMaxDistance, NavMesh.AllAreas))
            {
                s.worldNav = hit.position;
                s.onNav = true;
            }
            else
            {
                s.worldNav = s.worldDesired;
                s.onNav = false;
            }
            slots[i] = s; // struct-like update safeguard
        }
    }

    void LocalRelaxation()
    {
        // Fill inner holes by pulling nearest-in-angle outer neighbors one step.
        // Limit the number of moves per tick to keep motion local.
        int claims = 0;
        for (int i = 0; i < slots.Count && claims < maxClaimsPerTick; i++)
        {
            var free = slots[i];
            if (free.occupied) continue;

            // search for a candidate owner with bigger radius and similar angle
            int bestIdx = -1; float bestDelta = 999f;
            for (int j = i + 1; j < slots.Count; j++)
            {
                var s = slots[j];
                if (!s.occupied || s.owner == null) continue;
                float delta = Mathf.Abs(Mathf.DeltaAngle(s.angleDeg, free.angleDeg));
                if (delta < neighborClaimAngleDeg && delta < bestDelta && s.radius > free.radius + spacing * 0.2f)
                {
                    bestDelta = delta; bestIdx = j;
                }
            }

            if (bestIdx >= 0)
            {
                // move owner from bestIdx to i (one-step inward)
                var from = slots[bestIdx];
                var owner = from.owner;
                from.owner = null; from.occupied = false;
                free.owner = owner; free.occupied = true;
                owner.BindSlot(free.id);
                slots[bestIdx] = from; slots[i] = free;
                claims++;
            }
        }
    }

    void PushDestinations()
    {
        if (suspended) return;
        for (int i = 0; i < slots.Count; i++)
        {
            var s = slots[i];
            if (!s.occupied || s.owner == null) continue;

            if (useKinematicOffMesh && !s.onNav)
                s.owner.KinematicFollow(s.worldDesired);
            else
                s.owner.SetAgentTarget(s.onNav ? s.worldNav : s.worldDesired);
        }
    }
}
