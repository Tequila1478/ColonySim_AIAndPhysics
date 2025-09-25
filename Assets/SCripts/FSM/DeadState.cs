using UnityEngine;

public class DeadState : VillagerStateBase
{
    public DeadState(VillagerAI villager) : base(villager) { }
    public override bool CanChangeRole => false;


    public override void Enter()
    {
        villager.agent.enabled = false;
        villager.villagerData.isDead = true;

        VillageData.Instance.UpdateNumberOfVillagers();
    }

    public override void OnDropped()
    {
        //Doesn't change state, stays in this one instead
    }

    public override void UpdateEnergy()
    {

    }

    protected override void OnExecute()
    {
        //throw new System.NotImplementedException();
    }
}
