using UnityEngine;
using UnityEngine.AI;

public class WanderMoveState : IVillagerState
{
    private VillagerAI villager;
    private Vector3 target;

    private int repeatCount = 0;
    private int maxRepeats = 1;

    public WanderMoveState(VillagerAI villager, int maxRepeats = 0)
    {
        this.villager = villager;
        if (maxRepeats > 0)
            this.maxRepeats = maxRepeats;
    }

    public void Enter()
    {
        Debug.Log("Villager wandering move");
        villager.agent.enabled = true;
        if (repeatCount == 0)
            maxRepeats = Random.Range(1, 11);

        if (villager.TryGetRandomNavMeshPoint(villager.homePosition, villager.wanderRadius, out target))
        {
            villager.agent.SetDestination(target);
            if (villager.animator != null)
                villager.animator.SetBool(villager.moveBool, true);
        }
        Debug.Log("Max repeats for: " + maxRepeats);
    }

    public void Execute()
    {
        if (!villager.agent.pathPending &&
        villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, villager.reachThreshold))
        {
            Debug.Log("Villager starting wandering, repeat count: " + repeatCount + " incrementing repeatCount++");
            repeatCount++;

            if (repeatCount >= maxRepeats)
            {
                Debug.Log("Villager done wandering, changing to random role");
                villager.SetRole(villager.villagerData.GetRandomRole());
            }
            else
            {
                Debug.Log("Villager done moving, changing to idle");
                //villager.fsm.ChangeState(new WanderIdleState(villager, this), villager);
            }
        }
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

    public int GetRepeatCount() => repeatCount;
    public int GetMaxRepeats() => maxRepeats;
}