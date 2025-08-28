using UnityEngine;

public class WanderIdleState : IVillagerState
{
    private VillagerAI villager;
    private float idleTime, timer;

    public WanderIdleState(VillagerAI villager) { this.villager = villager; }

    public void Enter()
    {
        idleTime = Random.Range(villager.minIdleTime, villager.maxIdleTime);
        timer = 0f;
        if (villager.animator != null)
            villager.animator.SetBool(villager.moveBool, false);
    }

    public void Execute()
    {
        timer += Time.deltaTime;
        if (timer >= idleTime) 
        {
            PickNewState();
        }
    }

    private void PickNewState()
    {
        villager.SetRole(villager.villagerData.GetRandomRole());
    }

    public void Exit() { }

    public void OnDropped()
    {
        villager.fsm.ChangeState(new WanderMoveState(villager));
    }
}