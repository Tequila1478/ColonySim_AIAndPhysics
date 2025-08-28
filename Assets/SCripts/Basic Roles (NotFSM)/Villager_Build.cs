using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Villager_Build : MonoBehaviour
{
    [Header("References")]
    public Transform[] woodSources;     // array of wood nodes
    public Transform houseSite;         // house construction site

    [Header("Settings")]
    public float gatherTime = 3f;       // seconds to collect wood
    public float reachThreshold = 0.4f; // distance to consider “arrived”
    public bool startOnAwake = true;

    [Header("Colour")]
    public Color colour;

    [Header("Optional Animator")]
    public Animator animator;
    public string moveBool = "isMoving";

    private NavMeshAgent agent;
    private Transform currentTarget;
    private bool carryingWood = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (woodSources.Length == 0)
            Debug.LogWarning("No wood sources assigned!");

        if (houseSite == null)
            Debug.LogWarning("House site not assigned!");

        gameObject.GetComponent<SpriteRenderer>().color = colour;
    }

    private void OnEnable()
    {
        if (startOnAwake)
            StartBuilding();
    }

    private void Update()
    {
        // optional animator
        if (animator != null)
        {
            bool moving = agent.velocity.sqrMagnitude > 0.01f;
            animator.SetBool(moveBool, moving);
        }

        // Check if arrived
        if (agent.enabled && currentTarget != null && !agent.pathPending &&
            agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, reachThreshold))
        {
            if (!carryingWood && IsWoodSource(currentTarget))
            {
                StartCoroutine(GatherWood());
            }
            else if (carryingWood && currentTarget == houseSite)
            {
                DeliverWood();
            }
        }
    }

    #region Builder Logic
    private void StartNextTask()
    {
        // pick a random wood source
        if (woodSources.Length == 0) return;

        currentTarget = woodSources[Random.Range(0, woodSources.Length)];
        carryingWood = false;
        MoveTo(currentTarget.position);
    }

    private IEnumerator GatherWood()
    {
        agent.isStopped = true; // pause agent while gathering
        // optional: play gather animation here
        yield return new WaitForSeconds(gatherTime);

        carryingWood = true;
        currentTarget = houseSite;
        agent.isStopped = false;
        MoveTo(currentTarget.position);
    }

    private void DeliverWood()
    {
        // optional: increment house wood counter or update UI
        carryingWood = false;

        // loop back to next wood source
        StartNextTask();
    }

    private bool IsWoodSource(Transform target)
    {
        foreach (var wood in woodSources)
        {
            if (wood == target) return true;
        }
        return false;
    }

    private void MoveTo(Vector3 target)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning("Target not on NavMesh: " + target);
        }
    }

    public void StartBuilding()
    {
        StartNextTask();
    }

    public void PauseAI()
    {
        agent.isStopped = true;
    }

    public void ResumeAI()
    {
        agent.isStopped = false;
        if (currentTarget != null)
            MoveTo(currentTarget.position);
    }
    #endregion
}