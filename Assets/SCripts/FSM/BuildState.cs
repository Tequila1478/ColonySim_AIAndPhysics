using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BuildState : VillagerStateBase
{
    private ResourceObj targetNode;
    private BuildObj building;

    private float resourcesCarried;
    private bool carryingWood = false;

    private Transform currentMoveLocation;

    public BuildState(VillagerAI villager) : base(villager) 
    {
        rate = -Mathf.Clamp(Mathf.Pow(0.5f, (villager.villagerData.GetSkill(VillagerSkills.Build) - 1) / 4f), 0.01f, 1f);
        skillType = VillagerSkills.Build;
        float skillLevel = villager.villagerData.GetSkill(skillType);
        levelUpRate = Mathf.Clamp(Mathf.Pow(0.5f, skillLevel / 5f), 0.001f, 0.1f);

    }

    public override void Enter()
    {
        if (VillageData.Instance.lumberStores == null)
            Debug.LogWarning("No wood stores assigned!");

        if (VillageData.Instance.currentBuilding == null)
            VillageData.Instance.SetCurrentBuilding();

        targetNode = VillageData.Instance.lumberStores;
        building = VillageData.Instance.currentBuilding;

        StartBuilding();
    }


    public void StartBuilding()
    {
        StartNextTask();
    }

    public void StartNextTask()
    {
        if (targetNode == null) return;
        carryingWood = false;
        resourcesCarried = 0;

        currentMoveLocation = targetNode.transform;
        villager.MoveTo(currentMoveLocation.position);
    }

    private IEnumerator GatherWoodCoroutine()
    {
        villager.agent.isStopped = true; // pause agent while gathering
        // optional: play gather animation here
        yield return new WaitForSeconds(targetNode.gatherTime * MoodEffects.GetEffects(villager.villagerData.mood).workSpeedMultiplier);

        if (!carryingWood)
        {
            carryingWood = true;
            resourcesCarried = targetNode.GatherResource(GetGatherAmount());

            if (resourcesCarried == 0)
            {
                villager.villagerData.failedTaskRecently = true;
                Debug.Log("Resource deplenished");
                villager.SetRole(Villager_Role.Wander);
            }
        }

        carryingWood = true;

        currentMoveLocation = building.transform;
        villager.agent.isStopped = false;
        villager.MoveTo(currentMoveLocation.position);

        yield break;
    }

    private IEnumerator DeliverWoodCoroutine()
    {
        villager.agent.isStopped = true; // pause agent while gathering
        // optional: play gather animation here
        yield return new WaitForSeconds(building.buildTime * MoodEffects.GetEffects(villager.villagerData.mood).workEfficiencyMultiplier);

        building.ConstructBuilding(resourcesCarried);

        // optional: increment house wood counter or update UI
        carryingWood = false;
        resourcesCarried = 0;
        villager.agent.isStopped = false;
        villager.villagerData.completedTaskRecently = true;

        if (building.isComplete)
        {
            villager.SetRole(Villager_Role.Wander);
        }
        else
        // loop back to next wood source
        StartNextTask();
        yield break;
    }

    protected override void OnExecute()
    {
        if (villager.agent.pathPending) return;

        if (!carryingWood && villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, villager.reachThreshold))
        {
            // arrived at wood source -> gather
            villager.agent.isStopped = true;
            villager.StartCoroutine(GatherWoodCoroutine());
        }
        else if (carryingWood && villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, villager.reachThreshold))
        {
            // arrived at house -> deliver
            villager.agent.isStopped = true;
            villager.StartCoroutine(DeliverWoodCoroutine());
        }
        
        
    }

    private float GetGatherAmount()
    {
        float skillLevel = villager.villagerData.GetSkill(VillagerSkills.Build);

        return targetNode.gatherAmount * VillageData.Instance.GetSkillEffect(skillLevel);

    }
}
