using UnityEngine;

public class WanderIdleState : IVillagerState
{
    private VillagerAI villager;
    private float idleTime, timer;

    private WanderMoveState previousMoveState;

    public WanderIdleState(VillagerAI villager, WanderMoveState moveState = null) 
    { 
        this.villager = villager; 
        previousMoveState = moveState;
    }

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
        if (previousMoveState != null && previousMoveState.GetRepeatCount() < previousMoveState.GetMaxRepeats())
        {
           // villager.fsm.ChangeState(previousMoveState);
        }
        else
        {
            // Done: pick a new role
            villager.SetRole(villager.villagerData.GetRandomRole());
        }
    }

    public void Exit() { }

    public void OnDropped()
    {
        //villager.fsm.ChangeState(new WanderMoveState(villager), villager);
    }
}