using System.Collections;
using UnityEngine;

public class Research_ResearchingState : IVillagerSubState
{
    private ResearchState parent;
    private VillagerAI villager;
    private ResearchObj table;
    private float timer = 0;
    public Research_ResearchingState(ResearchState parent, VillagerAI villager, ResearchObj table)
    {
        this.parent = parent;
        this.villager = villager;
        this.table = table;
    }

    public void Enter()
    {
        // stop movement while researching
        villager.agent.isStopped = true;

        // reset timer
        timer = 0f;
    }

    private IEnumerator ResearchCoroutine()
    {
        // stop movement
        villager.agent.isStopped = true;

        // play research animation here if needed

        yield return new WaitForSeconds(table.researchTime);

        if (parent.researchCarried == 0)
        {
            parent.researchCarried = table.GatherResource(villager);
        }

        // gather research
        parent.StartMoveTo(parent.researchDropOffLocation.gameObject);
    }

    public void Execute()
    {
        timer += Time.deltaTime;

        if (timer >= table.researchTime)
        {
            // finish research
            if (parent.researchCarried == 0)
            {
                parent.researchCarried = GetGatherAmount();
            }

            // move to drop-off
            parent.StartMoveTo(parent.researchDropOffLocation.gameObject);
        }
    }

    private float GetGatherAmount()
    {
        float skillLevel = villager.villagerData.GetSkill(VillagerSkills.Research);

        return table.researchAmount * VillageData.Instance.GetSkillEffect(skillLevel);

    }

    public void Exit()
    {
        villager.agent.isStopped = false;
    }
}
