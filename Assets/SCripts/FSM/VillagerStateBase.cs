using UnityEngine;

public abstract class VillagerStateBase
{
    protected VillagerAI villager;

    private float tickInterval = 1f;
    private float tickTimer = 0f;

    public float moveSpeed = 5f;

    public float rate = 0.001f;
    public float levelUpRate = 0f;
    public VillagerSkills skillType = VillagerSkills.Heal;

    public virtual bool CanChangeRole => true;

    public VillagerStateBase(VillagerAI villager)
    {
        this.villager = villager;
        moveSpeed = villager.agent.speed * MoodEffects.GetEffects(villager.villagerData.mood).moveSpeedMultiplier;
    }

    public virtual void OnResourceDelivered()
    {
        // default: do nothing
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
        villager.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        OnExit();
    }

    public virtual void OnExit() { }

    // Default pickup/drop behavior

    public virtual void Dropped()
    {
        OnDropped();
    }
    public virtual void OnDropped()
    {
        int mask = 1 << LayerMask.NameToLayer("NoVillagerCollision");

        float radius = 1f; // 
        Collider2D[] hits = Physics2D.OverlapCircleAll(villager.transform.position, radius, mask);

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

    public virtual void PickUp()
    {
        OnPickUp();
    }
    public virtual void OnPickUp()
    {
        villager.SetRole(Villager_Role.PickedUp, forced: true);
    }

    protected virtual void UpdateSkill()
    {
        if (villager == null || villager.villagerData == null) return;

            // Gain skill scaled by rate and per-frame delta
            float gain = levelUpRate * Time.deltaTime;
            villager.villagerData.AddSkill(skillType, gain);
    }

    public virtual void UpdateEnergy()
    {
        if (villager == null) return;

        float sleepChance = villager.villagerData.IncrementEnergy(rate * GetMoodEnergyMultiplier());

        if (Random.value < sleepChance && villager.currentRole != Villager_Role.Sleep && villager.currentRole != Villager_Role.Eat)
        {
            villager.SetRole(Villager_Role.Sleep);
        }
    }

    protected virtual float GetMoodEnergyMultiplier()
    {
        if (villager == null || villager.villagerData == null) return 1f;

        return MoodEffects.GetEffects(villager.villagerData.mood).energyConsumptionMultiplier;
    }

    protected void MoveTowards(Vector2 target, float speed)
    {
        Rigidbody2D rb = villager.GetComponent<Rigidbody2D>();
        Vector2 direction = (target - rb.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    protected virtual (float timeMult, float amountMult) GetSkillImpact()
    {
        if (villager == null || villager.villagerData == null)
            return (1f, 1f); // fallback, no effect

        // Get this state's relevant skill
        float skill = villager.villagerData.GetSkill(skillType);

        // Normalize to 0–1
        float t = Mathf.Clamp01(skill / 100f);

        // Apply an easing function (ease-out cubic) rapid gains at low skill, diminishing returns later
        float curved = 1f - Mathf.Pow(1f - t, 3f);

        // Map to your desired ranges:
        float timeMult = Mathf.Lerp(1.5f, 0.5f, curved);

        // Amount multiplier:
        float amountMult = Mathf.Lerp(0.5f, 2.0f, curved);

        return (timeMult, amountMult);
    }
}
