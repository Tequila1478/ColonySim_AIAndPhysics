using UnityEngine;

public class DeadState : VillagerStateBase
{
    public DeadState(VillagerAI villager) : base(villager) { }

    public override void Enter()
    {
        villager.agent.enabled = false;

    }

    public override void OnDropped()
    {
        //Doesn't change state, stays in this one instead
    }

}
