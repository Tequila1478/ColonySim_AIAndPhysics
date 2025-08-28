using UnityEngine;
using UnityEngine.AI;

public class WanderMoveState : IVillagerState
{
    private VillagerAI villager;
    private Vector3 target;

    public WanderMoveState(VillagerAI villager) { this.villager = villager; }

    public void Enter()
    {
        if (villager.TryGetRandomNavMeshPoint(villager.homePosition, villager.wanderRadius, out target))
        {
            villager.agent.SetDestination(target);
            if (villager.animator != null)
                villager.animator.SetBool(villager.moveBool, true);
        }
    }

    public void Execute()
    {
        if (!villager.agent.pathPending &&
            villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, villager.reachThreshold))
            villager.fsm.ChangeState(new WanderIdleState(villager));
    }

    public void Exit()
    {
        villager.agent.ResetPath();
        if (villager.animator != null)
            villager.animator.SetBool(villager.moveBool, false);
    }

    public void OnDropped()
    {
        Enter();
    }
}