using UnityEngine;

public class DeadState : IVillagerState
{
    private VillagerAI villager;
    public DeadState(VillagerAI villager) { this.villager = villager; }

    public void Enter()
    {
        villager.agent.enabled = false;

    }

    public void Execute()
    {
        //throw new System.NotImplementedException();
    }

    public void Exit()
    {
       //throw new System.NotImplementedException();
    }

    public void OnDropped()
    {
        //throw new System.NotImplementedException();
    }

}
