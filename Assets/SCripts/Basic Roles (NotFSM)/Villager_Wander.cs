using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Villager_Wander : MonoBehaviour
{
    [Header("Wander Settings")]
    public float wanderRadius = 8f;               // how far from home they can roam
    public float minIdleTime = 1f;                // wait between wander destinations
    public float maxIdleTime = 4f;
    public float reachThreshold = 0.4f;           // considered "arrived" when within this distance
    public int sampleAttempts = 30;               // attempts to find a NavMesh point
    public bool startOnAwake = true;


    [Header("Colour")]
    public Color colour;

    [Header("Optional")]
    public Animator animator;                     // optional: drive walking animations
    public string animatorBool = "isMoving";      // parameter name to toggle

    private NavMeshAgent agent;
    private Vector3 homePosition;
    private Coroutine wanderRoutine;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        homePosition = transform.position;

        gameObject.GetComponent<SpriteRenderer>().color = colour;

    }

    void OnEnable()
    {
        if (startOnAwake)
            StartWandering();
    }

    void OnDisable()
    {
        StopWandering();
    }

    void Update()
    {
        // optional: update animator based on agent velocity
        if (animator != null && agent != null)
        {
            bool moving = agent.velocity.sqrMagnitude > 0.01f;
            animator.SetBool(animatorBool, moving);
        }
    }

    #region Wander logic
    public void StartWandering()
    {
        if (wanderRoutine == null)
            wanderRoutine = StartCoroutine(WanderLoop());
    }

    public void StopWandering()
    {
        if (wanderRoutine != null)
        {
            StopCoroutine(wanderRoutine);
            wanderRoutine = null;
        }
    }

    private IEnumerator WanderLoop()
    {
        while (true)
        {
            // wait until agent is enabled (useful if drag temporarily disables it)
            yield return new WaitUntil(() => agent != null && agent.enabled);

            // choose a random valid point on NavMesh
            if (TryGetRandomNavMeshPoint(homePosition, wanderRadius, out Vector3 target))
            {
                agent.SetDestination(target);
            }
            else
            {
                // couldn't sample a point; wait and try again
                yield return new WaitForSeconds(1f);
                continue;
            }

            // wait until agent arrives (or agent gets disabled)
            while (agent.enabled && (agent.pathPending || agent.remainingDistance > Mathf.Max(agent.stoppingDistance, reachThreshold)))
            {
                yield return null;
            }

            // small settle time at destination
            float idle = Random.Range(minIdleTime, maxIdleTime);

            // if agent was disabled mid-wait, go back to top of loop
            float elapsed = 0f;
            while (elapsed < idle)
            {
                if (agent == null || !agent.enabled) break;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }

    private bool TryGetRandomNavMeshPoint(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < sampleAttempts; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * radius;
            randomPoint.y = center.y; // keep same y-level for sampling (works for mostly-flat NavMesh)
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = center;
        return false;
    }
    #endregion
}