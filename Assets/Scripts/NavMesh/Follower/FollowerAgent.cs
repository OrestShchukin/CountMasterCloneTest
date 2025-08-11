using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class FollowerAgent : MonoBehaviour
{
    [Header("Look / Steering")]
    public Transform lookAtTarget;
    public float turnLerp = 10f;

    [Header("NavMesh steering")]
    public float repathDistance = 0.12f; // ε to re-issue SetDestination

    [Header("Off-mesh (kinematic) fallback")]
    public float kinematicSpeed = 4.5f;

    [HideInInspector] public int slotId = -1; // assigned by FormationManager

    NavMeshAgent agent;
    Vector3 lastIssuedTarget;
    bool jumping;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // manual rotation for nicer look
    }

    public void BindLook(Transform t) => lookAtTarget = t;
    public void BindSlot(int id) => slotId = id;

    public void SetTargetIfNeeded(Vector3 target)
    {
        if (!agent.enabled) return;
        if ((target - lastIssuedTarget).sqrMagnitude > repathDistance * repathDistance)
        {
            agent.SetDestination(target);
            lastIssuedTarget = target;
        }
    }

    public void SetAgentTarget(Vector3 target)
    {
        if (!agent.enabled)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 1.0f, NavMesh.AllAreas))
                transform.position = hit.position;
            agent.enabled = true;
        }
        SetTargetIfNeeded(target);
    }

    public void KinematicFollow(Vector3 desired)
    {
        if (agent.enabled) agent.enabled = false;

        Vector3 p = transform.position;
        Vector3 to = desired - p; to.y = 0f;
        float step = kinematicSpeed * Time.deltaTime;
        if (to.magnitude > step) p += to.normalized * step; else p = desired;
        transform.position = p;

        if (lookAtTarget)
        {
            var dir = lookAtTarget.position - transform.position; dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                var rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * turnLerp);
            }
        }
    }

    // --- Jump integration (e.g., DOTween DOJump) ---
    public void BeginJump(float expectedDuration)
    {
        if (jumping) return;
        jumping = true;
        if (agent.enabled) agent.enabled = false;
    }

    public void EndJump()
    {
        if (NavMesh.SamplePosition(transform.position, out var hit, 1.0f, NavMesh.AllAreas))
            transform.position = hit.position;
        agent.enabled = true;
        jumping = false;
    }

    void Update()
    {
        if (!lookAtTarget || jumping) return;
        var dir = lookAtTarget.position - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
        {
            var rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * turnLerp);
        }
    }
}