using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BuildState : IVillagerState
{
    private ResourceObj targetNode;
    private VillagerAI villager;
    private BuildObj building;

    private float resourcesCarried;
    private bool carryingWood = false;

    private Transform currentMoveLocation;

    public BuildState(VillagerAI villager)
    {
        this.villager = villager;
    }

    public void Enter()
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
        MoveTo(currentMoveLocation.position);
    }

    private IEnumerator GatherWoodCoroutine()
    {
        villager.agent.isStopped = true; // pause agent while gathering
        // optional: play gather animation here
        yield return new WaitForSeconds(targetNode.gatherTime);

        if (!carryingWood)
        {
            carryingWood = true;
            resourcesCarried = targetNode.GatherResource(GetGatherAmount());
            targetNode.incrementResource(-resourcesCarried);

            if (resourcesCarried == 0)
            {
                Debug.Log("Resource deplenished");
                villager.SetRole(Villager_Role.Wander);
            }
        }

        carryingWood = true;

        currentMoveLocation = building.transform;
        villager.agent.isStopped = false;
        MoveTo(currentMoveLocation.position);

        yield break;
    }

    private IEnumerator DeliverWoodCoroutine()
    {
        villager.agent.isStopped = true; // pause agent while gathering
        // optional: play gather animation here
        yield return new WaitForSeconds(building.buildTime);

        building.ConstructBuilding(resourcesCarried);

        // optional: increment house wood counter or update UI
        carryingWood = false;
        resourcesCarried = 0;
        villager.agent.isStopped = false;

        villager.villagerData.AddSkill(VillagerSkills.Build);

        if (building.isComplete)
        {
            villager.SetRole(Villager_Role.Wander);
        }
        else
        // loop back to next wood source
        StartNextTask();
        yield break;
    }

    private void MoveTo(Vector3 target)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 2f, NavMesh.AllAreas))
        {
            villager.agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning("Target not on NavMesh: " + target);
        }
    }

    public void Execute()
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

    public void Exit()
    {
        villager.agent.isStopped = false;
    }

    public void OnDropped()
    {
        MoveTo(currentMoveLocation.position);
    }
}