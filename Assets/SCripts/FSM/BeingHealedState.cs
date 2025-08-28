using UnityEngine;

public class BeingHealedState : IVillagerState
{
    private VillagerAI villager;

    public BeingHealedState(VillagerAI villager) { this.villager = villager; }

    public void Enter()
    {
        villager.agent.enabled = false;
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
        villager.agent.enabled = true;
    }

    public void OnDropped()
    {
        villager.SetRole(Villager_Role.Wander);
    }
}
