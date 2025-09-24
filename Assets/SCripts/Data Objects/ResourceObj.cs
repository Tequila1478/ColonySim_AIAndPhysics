using UnityEngine;

public class ResourceObj : MonoBehaviour, IRoleInteractable
{
    [Header("Gather Settings")]
    public string resourceType = "food";
    public float gatherTime = 6f;
    public float gatherAmount = 10f;
    public float maxAmount = 1000;
    public float currentAmount = 100f;

    public float GatherResource(float resourceAmount)
    {
        if (currentAmount - resourceAmount > 0)
            return resourceAmount;

        else
        {
            return currentAmount;
        }
    }

    public void incrementResource(float resourceAmount)
    {
        currentAmount += resourceAmount;
    }

    public void OnVillagerDropped(VillagerAI villager)
    {
        if (resourceType == "food")
            villager.SetRole(Villager_Role.Eat);

        else if (resourceType == "lumber")
            villager.SetRole(Villager_Role.Build);

        else if (resourceType == "research")
            villager.SetRole(Villager_Role.Research);
        else
            Debug.Log("Incorrect resource type. Must be one of 'food' or 'lumber' or 'research'");
    }

    private void Update()
    {
        if (resourceType == "food")
            currentAmount = VillageData.Instance.foodCount;

        else if (resourceType == "lumber")
            currentAmount = VillageData.Instance.lumberCount;

        else if (resourceType == "research")
            currentAmount = VillageData.Instance.researchCount;
        else
            Debug.Log("Incorrect resource type. Must be one of 'food' or 'lumber' or 'research'");
    }
}
