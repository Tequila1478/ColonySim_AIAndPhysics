using UnityEngine;

public class PickupState : VillagerStateBase
{
    public PickupState(VillagerAI villager) : base(villager) { }

    public override void Enter()
    {

        if (villager.agent != null)
        {
            villager.agent.enabled = false; // stop NavMesh movement
        }

        if (villager.animator != null)
        {
            villager.animator.SetBool(villager.moveBool, false);
        }
        villager.villagerData.wasPickedupRecently = true;
    }


    public override void Exit()
    {
        if (villager.agent != null)
        {
            villager.agent.enabled = true;
        }
    }

    protected override void OnExecute()
    {
//throw new System.NotImplementedException();
    }
}
