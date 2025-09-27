using Mono.Cecil;
using UnityEngine;

public class GatherObj : MonoBehaviour, IRoleInteractable
{
    [Header("Gather Settings")]
    public string resourceType = "food";
    public float gatherTime = 3f;
    public float gatherAmount = 10f;
    public float maxAmount = 100f;
    public float currentAmount = 100f;
    public bool isResourceDepot = false;
    public GameObject resourcePrefab;

    public float GatherResource(float resourceAmount)
    {
        if(currentAmount - resourceAmount > 0)
        return resourceAmount;

        else
        {
            return currentAmount;
        }
    }

    public void incrementResource(float resourceAmount)
    {
        currentAmount += resourceAmount;
        if (isResourceDepot)
        {
            if(resourceType == "food")
            {
                VillageData.Instance.IncrementFood(resourceAmount);
            }
            else if(resourceType == "lumber")
            {
                VillageData.Instance.IncrementLumber(resourceAmount);

            }
            else if(resourceType == "research")
            {
                VillageData.Instance.IncrementResearch(resourceAmount);
            }
            else
            {
                Debug.Log("No resource type set correctly");
            }
        }
    }

    void IRoleInteractable.OnVillagerDropped(VillagerAI villager)
    {
        Debug.Log("Dropped on type GatherOBj");
        if (resourceType == "food")
        {
            villager.villagerData.gatherType = "food";
            villager.SetRole(Villager_Role.Gather);
        }
        else if (resourceType == "lumber")
        {
            villager.villagerData.gatherType = "lumber";
            villager.SetRole(Villager_Role.Gather);
        }
        else
            Debug.Log("Incorrect resource type. Must be one of 'food' or 'lumber' or 'research'");
    }
}
