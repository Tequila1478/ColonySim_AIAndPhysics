using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EatState : VillagerStateBase
{
    private ResourceObj foodDepot;
    private float carryingResource;

    public bool holdingResources = false;


    private Transform currentMoveLocation;

    public EatState(VillagerAI villager) : base(villager)
    {
        rate = -0.0001f;
    }

    public override void Enter()
    {
        StartNextGather();
    }

    public void StartNextGather()
    {
        //Find resource to gather
        foodDepot = VillageData.Instance.foodStores;
        currentMoveLocation = foodDepot.transform;

        
        villager.MoveTo(currentMoveLocation.position);
    }
    protected override void OnExecute()
    {
        // Check if we reached destination
        if (villager.agent.enabled && currentMoveLocation != null && !villager.agent.pathPending && villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, villager.reachThreshold))
        {
            
            villager.StartCoroutine(GatherResource());
            
        }
    }

    private IEnumerator GatherResource()
    {
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
                if (villager.villagerData.isSick)
                {
                    villager.SetRole(Villager_Role.Sick);
                }
                else
                {
                    villager.canTryEat = false;
                villager.villagerData.hasEatenRecently = true;
                    villager.eatCooldown = 0;
                    villager.SetRole(Villager_Role.Wander);

                }
            }
        

        currentMoveLocation = VillageData.Instance.GetDropOffLocation(villager.villagerData.gatherType);
        villager.agent.SetDestination(currentMoveLocation.position);
        villager.agent.isStopped = false;
    }

}