using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Villager_Gather : MonoBehaviour
{
    [Header("References")]
    public Transform dropOffLocation;
    public Transform[] resourceNodes;

    [Header("Settings")]
    public float gatherTime = 3f;      // time to gather resource
    public float reachThreshold = 0.4f;

    [Header("Colour")]
    public Color colour;

    [Header("Optional Animator")]
    public Animator animator;
    public string moveBool = "isMoving";

    private NavMeshAgent agent;
    private Transform currentTarget;
    private bool carryingResource = false;


    private void Start()
    {
        StartGathering();
    }
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (resourceNodes.Length == 0)
            Debug.LogWarning("No resource nodes assigned!");

        gameObject.GetComponent<SpriteRenderer>().color = colour;

    }

    private void Update()
    {
        // optional animator
        if (animator != null)
        {
            bool moving = agent.velocity.sqrMagnitude > 0.01f;
            animator.SetBool(moveBool, moving);
        }

        // Check if we reached destination
        if (agent.enabled && currentTarget != null && !agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, reachThreshold))
        {
            if (!carryingResource)
            {
                // Arrived at resource
                StartCoroutine(GatherResource());
            }
            else
            {
                // Arrived at drop-off
                DeliverResource();
            }
        }
    }

    #region Gathering logic
    private void StartNextGather()
    {
        if (resourceNodes.Length == 0) return;

        // pick a random resource node
        currentTarget = resourceNodes[Random.Range(0, resourceNodes.Length)];
        carryingResource = false;
        agent.SetDestination(currentTarget.position);
    }

    private IEnumerator GatherResource()
    {
        agent.isStopped = true; // pause agent while gathering
        // optional: play gather animation here

        yield return new WaitForSeconds(gatherTime);

        carryingResource = true;
        currentTarget = dropOffLocation;
        agent.SetDestination(currentTarget.position);
        agent.isStopped = false;
    }

    private void DeliverResource()
    {
        // optional: add resource to inventory, update UI, etc.
        carryingResource = false;

        // loop back to next resource
        StartNextGather();
    }
    #endregion

    public void StartGathering()
    {
        StartNextGather();
    }

    public void PauseAI()
    {
        agent.isStopped = true;
    }

    public void ResumeAI()
    {
        agent.isStopped = false;
        if (currentTarget != null)
            agent.SetDestination(currentTarget.position);
    }
}