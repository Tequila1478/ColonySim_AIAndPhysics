using System.Collections;
using UnityEngine;

public class Research_DeliverResearchState : IVillagerSubState
{
    private ResearchState parent;
    private VillagerAI villager;
    private ResourceObj dropOff;
    private float researchCarried;

    public Research_DeliverResearchState(ResearchState parent, VillagerAI villager, ResourceObj dropOff, ref float researchCarried)
    {
        this.parent = parent;
        this.villager = villager;
        this.dropOff = dropOff;
        this.researchCarried = researchCarried;
    }

    public void Enter()
    {
        villager.StartCoroutine(DeliverCoroutine());
    }

    private IEnumerator DeliverCoroutine()
    {
        villager.agent.isStopped = true;
        yield return new WaitForSeconds(dropOff.gatherTime);
        //if (parent.researchCarried > 0)
        //{
        //    VillageData.Instance.IncrementResearch(parent.researchCarried);
        //    parent.researchCarried = 0;

        //}
        //villager.villagerData.AddSkill(VillagerSkills.Research);


        //parent.StartMoveTo(parent.table.gameObject);
    }

    public void Execute()
    {
        // nothing here; coroutine handles timing
    }

    public void Exit()
    {
        villager.villagerData.completedTaskRecently = true;
        villager.agent.isStopped = false;
    }
}
