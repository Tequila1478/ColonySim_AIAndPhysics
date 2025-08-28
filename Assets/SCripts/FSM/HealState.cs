using UnityEngine;
using UnityEngine.AI;

public class HealState : IVillagerState
{
    private VillagerAI villager;
    private Villager target;

    private float healDuration;   // how long it takes to heal
    private float healTimer;
    private bool isHealing = false;

    public HealState(VillagerAI villager)
    {
        this.villager = villager;
    }

    public void Enter()
    {
        villager.agent.enabled = true;
        villager.agent.isStopped = false;

        // Pick a sick villager to heal
        target = VillageData.Instance.GetSickVillager();
        if (target == null)
        {
            Debug.Log("No sick villagers available");
            villager.SetRole(Villager_Role.Wander);
            return;
        }

        // Compute heal duration based on healer's skill
        float skillLevel = villager.villagerData.GetSkill(VillagerSkills.Heal);
        healDuration = VillageData.Instance.healTime / VillageData.Instance.GetSkillEffect(skillLevel);

        // Mark the target as being healed
        target.GetComponent<VillagerAI>().SetRole(Villager_Role.BeingHealed);

        // Move towards the target
        villager.agent.SetDestination(target.transform.position);
        isHealing = false;
    }

    public void Execute()
    {
        if (target == null)
        {
            villager.SetRole(Villager_Role.Wander);
            return;
        }

        // If healing hasn't started yet, move to the target
        if (!isHealing)
        {
            // Continuously update destination in case target moves
            villager.agent.SetDestination(target.transform.position);

            if (!villager.agent.pathPending &&
                villager.agent.remainingDistance <= villager.reachThreshold)
            {
                // Arrived at target
                villager.agent.isStopped = true;
                healTimer = healDuration;
                isHealing = true;
            }
        }
        else
        {
            // Healing in progress
            healTimer -= Time.deltaTime;
            if (healTimer <= 0f)
            {
                FinishHealing();
            }
        }
    }

    private void FinishHealing()
    {
        if (target != null)
        {
            target.IncrementHealth(VillageData.Instance.healAmount);
            //target.GetComponent<VillagerAI>().SetRole(Villager_Role.Wander); // Reset target's role
        }

        villager.SetRole(Villager_Role.Wander); // Reset healer's role
    }

    public void Exit()
    {
        villager.agent.isStopped = false;
        isHealing = false;
        target = null;
    }

    public void OnDropped()
    {
        // Restart moving to target if villager is dropped
        Enter();
    }
}