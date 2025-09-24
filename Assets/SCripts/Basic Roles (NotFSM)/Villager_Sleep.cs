using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Villager_Sleep : MonoBehaviour
{
    [Header("Sleep Settings")]
    public Transform sleepLocation;      // assigned sleeping spot
    public float reachThreshold = 0.3f;  // distance to consider "at bed"
    public float recoveryRate = 5f;      // tiredness recovered per second

    [Header("Stats")]
    public float health = 100f;
    public float tiredness = 50f;

    [Header("Optional")]
    public Animator animator;
    public string sleepBool = "isSleeping";

    private NavMeshAgent agent;
    private bool isSleeping = false;
    private bool isOccupied = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void OnEnable()
    {
        StartCoroutine(SleepRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator SleepRoutine()
    {
        // 1. Check if there’s a sleep location
        if (sleepLocation == null)
        {
            Debug.LogWarning($"{name} has no sleep location assigned!");
            yield break;
        }

        // 2. Check if location is free (for now we just check occupancy flag)
        if (isOccupied)
        {
            Debug.Log($"{name} cannot sleep, bed is occupied.");
            yield break;
        }
        isOccupied = true; // claim it

        // 3. Walk to sleep location
        agent.isStopped = false;
        agent.SetDestination(sleepLocation.position);

        while (agent.pathPending || agent.remainingDistance > reachThreshold)
            yield return null;

        // 4. Enter sleeping state
        isSleeping = true;
        agent.isStopped = true;

        if (animator != null)
            animator.SetBool(sleepBool, true);

        // 5. Sleep until recovered
        while (tiredness < health)
        {
            tiredness += recoveryRate * Time.deltaTime;
            yield return null;
        }

        // 6. Wake up
        WakeUp();
    }

    private void WakeUp()
    {
        isSleeping = false;
        isOccupied = false;

        if (animator != null)
            animator.SetBool(sleepBool, false);

        Debug.Log($"{name} woke up refreshed!");

        // TODO: switch back to Wander or Work state here
        GetComponent<Villager_Wander>()?.StartWandering();
    }
}