using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EatState : IVillagerState
{
    private VillagerAI villager;
    private ResourceObj foodDepot;
    private float carryingResource;

    public bool holdingResources = false;


    private Transform currentMoveLocation;

    public EatState(VillagerAI villager) { this.villager = villager; }

    public void Enter()
    {
        StartGathering();
    }

    public void StartGathering()
    {
        StartNextGather();
    }

    public void StartNextGather()
    {
        //Find resource to gather
        foodDepot = VillageData.Instance.foodStores;
        currentMoveLocation = foodDepot.transform;

        
        MoveTo(currentMoveLocation.position);
    }
    public void Execute()
    {
        // Check if we reached destination
        if (villager.agent.enabled && currentMoveLocation != null && !villager.agent.pathPending && villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, villager.reachThreshold))
        {
            
            villager.StartCoroutine(GatherResource());
            
        }
    }

    private void MoveTo(Vector3 pos)
    {
        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            villager.agent.SetDestination(hit.position);
            villager.agent.isStopped = false;
        }
    }
    private IEnumerator GatherResource()
    {
        Debug.Log("eating food");
        villager.agent.isStopped = true; // pause agent while gathering
        // optional: play gather animation here

        yield return new WaitForSeconds(foodDepot.gatherTime);

            carryingResource = foodDepot.GatherResource(foodDepot.gatherAmount);
            foodDepot.incrementResource(-carryingResource);

            villager.villagerData.IncrementFood(carryingResource);


            if (carryingResource == 0)
            {
                Debug.Log("No Food available");
                villager.SetRole(Villager_Role.Wander);
            }

        carryingResource = 0;

            if(villager.villagerData.hunger >= 100)
            {
                Debug.Log("Villager full");
                if (villager.villagerData.isSick)
                {
                    villager.SetRole(Villager_Role.Sick);
                }
                else
                {
                    villager.canTryEat = false;
                    villager.eatCooldown = 0;
                    villager.SetRole(Villager_Role.Wander);

                }
            }
        

        currentMoveLocation = VillageData.Instance.GetDropOffLocation(villager.villagerData.gatherType);
        villager.agent.SetDestination(currentMoveLocation.position);
        villager.agent.isStopped = false;
    }

    
    public void Exit()
    {
        villager.agent.enabled = true;
        villager.agent.isStopped = false;
    }

    public void OnDropped()
    {
        MoveTo(currentMoveLocation.position);
    }
}