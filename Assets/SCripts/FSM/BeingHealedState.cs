using UnityEngine;

public class BeingHealedState : IVillagerState
{
    private VillagerAI villager;

    public BeingHealedState(VillagerAI villager) { this.villager = villager; }

    public void Enter()
    {
        villager.agent.isStopped = true;
    }

    public void Execute()
    {
        if (!villager.villagerData.isSick)
        {
            villager.SetRole(Villager_Role.Wander);
        }
    }

    public void Exit()
    {
        villager.agent.isStopped = false;
    }

    public void OnDropped()
    {
        villager.SetRole(Villager_Role.Wander);
    }
}
