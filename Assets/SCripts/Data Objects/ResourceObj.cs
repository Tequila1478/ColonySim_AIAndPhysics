using UnityEngine;

public class ResourceObj : MonoBehaviour, IRoleInteractable
{
    [Header("Gather Settings")]
    public string resourceType = "food";
    public float gatherTime = 6f;
    public float gatherAmount = 10f;
    public float maxAmount = 1000;
    public float currentAmount = 100f;
    public GameObject resourcePrefab;

    public float GatherResource(float resourceAmount)
    {
        float gathered = Mathf.Min(resourceAmount, currentAmount);

        currentAmount -= gathered;

        // Send updated amount back to VillageData
        switch (resourceType)
        {
            case "food":
                VillageData.Instance.foodCount = currentAmount;
                break;
            case "lumber":
                VillageData.Instance.lumberCount = currentAmount;
                break;
            case "research":
                VillageData.Instance.researchCount = currentAmount;
                break;
        }

        return gathered;
    }

    public void OnVillagerDropped(VillagerAI villager)
    {
        switch (resourceType)
        {
            case "food":
                villager.SetRole(Villager_Role.Eat);
                break;
            case "lumber":
                villager.SetRole(Villager_Role.Build);
                break;
            case "research":
                villager.SetRole(Villager_Role.Research);
                break;
            default:
                Debug.LogWarning("Incorrect resource type: " + resourceType);
                break;
        }
    }

    private void Start()
    {
        // Initialize with the starting value from VillageData
        switch (resourceType)
        {
            case "food":
                currentAmount = VillageData.Instance.foodCount;
                break;
            case "lumber":
                currentAmount = VillageData.Instance.lumberCount;
                break;
            case "research":
                currentAmount = VillageData.Instance.researchCount;
                break;
        }
    }
}
