using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GatherState : VillagerStateBase
{
    private GatherObj targetNode;
    private float carryingResource;

    public bool holdingResources = false;

    private Transform currentMoveLocation;

    public GatherState(VillagerAI villager) : base(villager) 
    {
        rate = -Mathf.Clamp(Mathf.Pow(0.5f, (villager.villagerData.GetSkill(VillagerSkills.Gather) - 1) / 4f), 0.01f, 1f);
        skillType = VillagerSkills.Gather;
        float skillLevel = villager.villagerData.GetSkill(skillType);
        levelUpRate = Mathf.Clamp(Mathf.Pow(0.5f, skillLevel / 5f), 0.0001f, 0.1f);

    }

    public override void Enter()
    {
        StartNextGather();
    }

    public void StartNextGather()
    {
        Debug.Log($"Villager: {villager.name}, VillageData.Instance: {VillageData.Instance}, gatherType: {villager.villagerData.gatherType}");
        Debug.Log($"Villager: {villager.name}, CheckGatherPoints output: {VillageData.Instance.CheckGatherPoints(villager.villagerData.gatherType)}");        //If there is no gather points available
        if (VillageData.Instance.CheckGatherPoints(villager.villagerData.gatherType) == 0)
        {
            Debug.Log($"Villager: {villager?.name}, No available gather points");
            //Return to idling
            villager.SetRole(Villager_Role.Wander);
            return;
        }

        //Find resource to gather
        targetNode = VillageData.Instance.GetRandomGatherPoint(villager.villagerData.gatherType);
        currentMoveLocation = targetNode.transform;

        //Check if carrying any resources?
        carryingResource = 0;
        holdingResources = false;

        villager.MoveTo(currentMoveLocation.position);
    }
    protected override void OnExecute()
    {
        // Check if we reached destination
        if (villager.agent.enabled && currentMoveLocation != null && !villager.agent.pathPending && villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, villager.reachThreshold))
        {
            if (carryingResource == 0)
            {
                // Arrived at resource
                villager.StartCoroutine(GatherResource());
            }
            else
            {
                // Arrived at drop-off
                DeliverResource();
            }
        }
    }

    private IEnumerator GatherResource()
    {
        villager.agent.isStopped = true; // pause agent while gathering
        // optional: play gather animation here

        yield return new WaitForSeconds(targetNode.gatherTime * MoodEffects.GetEffects(villager.villagerData.mood).workSpeedMultiplier);

        if (!holdingResources)
        {
            holdingResources = true;
            carryingResource = targetNode.GatherResource(GetGatherAmount());
            targetNode.incrementResource(-carryingResource);

            if (carryingResource == 0)
            {
                Debug.Log("Resource deplenished");
                villager.villagerData.failedTaskRecently = true;
                villager.SetRole(Villager_Role.Wander);
            }
        }

        currentMoveLocation = VillageData.Instance.GetDropOffLocation(villager.villagerData.gatherType);
        villager.agent.SetDestination(currentMoveLocation.position);
        villager.agent.isStopped = false;
    }

    private float GetGatherAmount()
    {
        float skillLevel = villager.villagerData.GetSkill(VillagerSkills.Gather);

        return targetNode.gatherAmount * VillageData.Instance.GetSkillEffect(skillLevel) * MoodEffects.GetEffects(villager.villagerData.mood).workEfficiencyMultiplier;

    }
    private void DeliverResource()
    {
        // optional: add resource to inventory, update UI, etc.
        if (villager.villagerData.gatherType == "food")
        {
            VillageData.Instance.IncrementFood(carryingResource);
        }
        else
        {
            VillageData.Instance.IncrementLumber(carryingResource);
        }
        carryingResource = 0;
        holdingResources = false;
        villager.villagerData.completedTaskRecently = true;



        // loop back to next resource
        StartNextGather();
    }
}