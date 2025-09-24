using UnityEngine;

public abstract class VillagerStateBase
{
    protected VillagerAI villager;

    private float tickInterval = 1f;
    private float tickTimer = 0f;

    public float rate = 0.001f;
    public float levelUpRate = 0f;
    public VillagerSkills skillType = VillagerSkills.Heal;

    public VillagerStateBase(VillagerAI villager)
    {
        this.villager = villager;
    }

    // Core lifecycle
    public virtual void Enter() { }
    public virtual void Execute() {
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            UpdateEnergy();
            UpdateSkill();
        }

        OnExecute();
    }

    protected abstract void OnExecute();

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

    protected virtual void UpdateSkill()
    {
        if (villager == null || villager.villagerData == null) return;

            // Gain skill scaled by rate and per-frame delta
            float gain = levelUpRate * Time.deltaTime;
            villager.villagerData.AddSkill(skillType, gain);
    }

    protected virtual void UpdateEnergy()
    {
        if (villager == null) return;

        float sleepChance = villager.villagerData.IncrementEnergy(rate * GetMoodEnergyMultiplier());

        if (Random.value < sleepChance && villager.currentRole != Villager_Role.Sleep)
        {
            villager.SetRole(Villager_Role.Sleep);
        }
    }

    protected virtual float GetMoodEnergyMultiplier()
    {
        if (villager == null || villager.villagerData == null) return 1f;

        return MoodEffects.GetEffects(villager.villagerData.mood).energyConsumptionMultiplier;
    }
}
