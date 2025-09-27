using UnityEngine;
using UnityEngine.AI;

public class ResourceObjData : MonoBehaviour
{
    private float checkInterval = 1f;
    private bool isBuild;

    public bool isLegitimateDelivery = false;

    void Start()
    {
        InvokeRepeating(nameof(CheckNavMesh), checkInterval, checkInterval);
    }

    void CheckNavMesh()
    {
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(transform.position, out hit, 2, NavMesh.AllAreas))
        {
            GetComponent<Rigidbody2D>().gravityScale = 1f;
            Debug.Log($"{name} left NavMesh, destroying");
            RemoveObj();
        }
    }

    public string type = "food";
    public float amount;

    private GameObject ownerState;

    public void RemoveOwner()
    {
        ownerState = null;
    }
    public void Init(string resourceType, float resourceAmount, GameObject owner, bool isBuildingResource = false)
    {
        type = resourceType;
        amount = resourceAmount;
        ownerState = owner;
        if(type == "Research")
            VillageData.Instance.AddToLooseResearchList(this);
        else
            VillageData.Instance.AddToLooseResourceList(this);
        isBuild = isBuildingResource;
    }

    public void UpdateOwnerState(GameObject newOwner)
    {
        ownerState = newOwner;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (type)
        {
            case "food":
                if (other.CompareTag("FoodDropOff"))
                {
                    VillageData.Instance.IncrementFood(amount);
                    RemoveObj();
                }
                break;

            case "lumber":
                if (isBuild)
                {
                    if (other.CompareTag("Building"))
                    {
                        isLegitimateDelivery = true;
                        RemoveObj();
                    }
                }
                else 
                {
                    if (other.CompareTag("LumberDropOff"))
                    {
                        VillageData.Instance.IncrementLumber(amount);
                        RemoveObj();
                    }
                }
                break;

            case "research":
                if (other.CompareTag("ResearchDropOff"))
                {
                    VillageData.Instance.IncrementResearch(amount);
                    RemoveObj();
                }
                break;

            default:
                Debug.LogWarning($"Unknown resource type: {type}");
                break;
        }

    }

    private void RemoveObj()
    {
        if (type == "research") 
        { 
            VillageData.Instance.RemoveFromLooseResearchList(this);
        
            if (ownerState != null)
            {
                ownerState.GetComponent<VillagerAI>().OnResourceDelivered();
            }
        }

        else
        {
            VillageData.Instance.RemoveFromLooseResourceList(this);

            if (ownerState != null)
            {
                ownerState.GetComponent<VillagerAI>().OnResourceDelivered();
            }
        }
            
        Destroy(gameObject);
    }
}
