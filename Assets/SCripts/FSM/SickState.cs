using UnityEngine;
using UnityEngine.AI;

public class SickState : IVillagerState
{
    private VillagerAI villager;
    private Vector3 target;
    private float idleTimer;
    private bool isIdle;

    // Configurable values
    private float wanderRadius = 2f;
    private float minIdleTime = 1f;
    private float maxIdleTime = 4f;
    private float reachThreshold = 0.4f;

    public SickState(VillagerAI villager) { this.villager = villager; }

    public void Enter()
    {        
        SetNewDestination();
    }

    public void Execute()
    {
        if (villager.agent == null || !villager.agent.enabled) return;

        // Check animator
        if (villager.animator != null)
            villager.animator.SetBool("isMoving", villager.agent.velocity.sqrMagnitude > 0.01f);

        if (!isIdle)
        {
            if (!villager.agent.pathPending &&
                villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, reachThreshold))
            {
                // Arrived, start idle
                idleTimer = Random.Range(minIdleTime, maxIdleTime);
                isIdle = true;
            }
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                isIdle = false;
                SetNewDestination();
            }
        }
    }

    public void Exit()
    {
        // Clean up when leaving state (optional)
    }

    public void OnDropped()
    {
        SetNewDestination();
    }

    private void SetNewDestination()
    {
        if (VillageData.Instance.hospitalLocation == null) return;

        Vector3 center = VillageData.Instance.hospitalLocation.transform.position;
        for (int i = 0; i < 30; i++) // sample attempts
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * wanderRadius;
            randomPoint.y = center.y; // keep ground-level sampling

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                target = hit.position;
                villager.agent.SetDestination(target);
                return;
            }
        }

        // fallback if no point found
        target = center;
        villager.agent.SetDestination(target);
    }
}