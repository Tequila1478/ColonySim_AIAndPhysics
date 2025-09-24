using UnityEngine;

public abstract class VillagerStateBase
{
    protected VillagerAI villager;

    public VillagerStateBase(VillagerAI villager)
    {
        this.villager = villager;
    }

    // Core lifecycle
    public virtual void Enter() { }
    public virtual void Execute() { }
    public virtual void Exit() {
        villager.agent.enabled = true;
        villager.agent.isStopped = false;
    }

    // Default pickup/drop behavior
    public virtual void OnDropped()
    {
        float radius = 0.1f; // 
        Collider2D[] hits = Physics2D.OverlapCircleAll(villager.transform.position, radius);

        Collider2D closestHit = null;
        float closestDist = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            var interactable = hit.GetComponent<IRoleInteractable>();
            if (interactable != null)
            {
                float dist = Vector2.Distance(villager.transform.position, hit.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestHit = hit;
                }
            }
        }

        if (closestHit != null)
        {
            Debug.Log("Dropped on closest interactable: " + closestHit.name);
            var interactable = closestHit.GetComponent<IRoleInteractable>();
            interactable.OnVillagerDropped(villager);
        }
        else
        {
            Debug.Log("No interactable nearby");
            villager.SetRole(villager.villagerData.GetRandomRole());
        }
    }

    public virtual void OnPickUp()
    {
        villager.SetRole(Villager_Role.PickedUp);
    }
}
