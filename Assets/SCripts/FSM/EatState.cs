using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EatState : VillagerStateBase
{
    private ResourceObj foodDepot;
    private float carryingResource;

    private bool isGathering = false;

    public bool holdingResources = false;

    public bool hasEaten = false;


    private Transform currentMoveLocation;
    public override bool CanChangeRole => hasEaten;

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
        if (foodDepot != null)
        {
            currentMoveLocation = foodDepot.transform;
            villager.MoveTo(currentMoveLocation.position);
        }
        else
        {
            Debug.Log("No food stores set in VIllageData");
        }
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
        if (isGathering) yield break;
        isGathering = true;

        // Check if there's any food before gathering
        if (foodDepot == null || foodDepot.currentAmount <= 0)
        {
            Debug.Log($"{villager.name} tried to eat, but no food is available.");
            villager.eatCooldown = 0; // reset cooldown
            villager.SetRole(Villager_Role.Wander);
            villager.canTryEat = false;
            isGathering = false;
            hasEaten = true;
            yield break;
        }

        villager.agent.isStopped = true;

        yield return new WaitForSeconds(foodDepot.gatherTime);
        // Calculate how much food is needed to fill hunger
        float neededFood = 100f - villager.villagerData.hunger;
        float gatheredFood = foodDepot.GatherResource(neededFood);

        hasEaten = true;
        // Increment villager's hunger by exactly what they ate
        villager.villagerData.IncrementFood(gatheredFood);

        villager.canTryEat = false;
        villager.eatCooldown = 0f;

        // If nothing was gathered (foodDepot empty)
        if (gatheredFood <= 0f)
        {
            Debug.Log("No Food available");
            villager.SetRole(Villager_Role.Wander);
            isGathering = false;
            yield break;
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
                villager.villagerData.hasEatenRecently = true;
                    villager.eatCooldown = 0;
                    villager.SetRole(Villager_Role.Wander);

                }
            }
        


        currentMoveLocation = VillageData.Instance.GetDropOffLocation(villager.villagerData.gatherType);
        villager.agent.SetDestination(currentMoveLocation.position);
        villager.agent.isStopped = false;

        isGathering = false;
    }

}