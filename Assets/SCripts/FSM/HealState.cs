using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements.Experimental;

public class HealState : VillagerStateBase
{
    private Villager target;
    private float healDuration;
    private float healTimer;
    private bool isHealing = false;

    public HealState(VillagerAI villager) : base(villager) { }

    public override void Enter()
    {
        Debug.Log("Finding villager to heal");
        target = VillageData.Instance.GetSickVillager();

        if (target == null)
        {
            villager.SetRole(Villager_Role.Wander);
            return;
        }

        // Mark as being healed
        target.GetComponent<VillagerAI>().isBeingHealed = true;

        // Compute heal duration
        float skillLevel = villager.villagerData.GetSkill(VillagerSkills.Heal);
        healDuration = VillageData.Instance.hospitalObj.healDuration / VillageData.Instance.GetSkillEffect(skillLevel);

        // Start moving to target
        villager.agent.isStopped = false;
        villager.MoveTo(target.transform.position);
        isHealing = false;
    }

    public override void Execute()
    {
        if (target == null)
        {
            villager.SetRole(Villager_Role.Wander);
            return;
        }
        // Update destination if target moves
        if (!isHealing)
        {
            villager.MoveTo(target.transform.position);

            if (!villager.agent.pathPending &&
                villager.agent.remainingDistance <= villager.reachThreshold)
            {
                // Arrived → start healing
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
        Debug.Log("Healing finished");
            target.IncrementHealth(VillageData.Instance.hospitalObj.healAmount);
            target.GetComponent<VillagerAI>().isBeingHealed = false;
            target.isSick = false;

        villager.SetRole(Villager_Role.Wander); // Reset healer's role
    }

    public override void Exit()
    {
        villager.agent.isStopped = false;
        isHealing = false;
        target = null;
    }

    public override void OnDropped()
    {
        if (target != null)
        {
            var targetAI = target.GetComponent<VillagerAI>();
            if (targetAI != null)
            {
                targetAI.isBeingHealed = false;
                targetAI.villagerData.isSick = true; // still sick if interrupted
                targetAI.SetRole(Villager_Role.Sick);
            }
        }

        // Exit the healer from HealState and pick a new random role
        villager.SetRole(villager.villagerData.GetRandomRole());
    }

    public override void OnPickUp()
    {
        if (target != null)
        {
            var targetAI = target.GetComponent<VillagerAI>();
            if (targetAI != null)
            {
                targetAI.isBeingHealed = false;
                targetAI.villagerData.isSick = true; // still sick if interrupted
                targetAI.SetRole(Villager_Role.Sick);
            }
        }
        villager.fsm.ChangeState(new PickupState(villager));
    } 
}