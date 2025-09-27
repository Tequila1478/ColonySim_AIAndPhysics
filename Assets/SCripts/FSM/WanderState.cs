using UnityEngine;

public class WanderState : VillagerStateBase
{
    private float idleTime, timer;
    private Vector3 target;

    private int repeatCount = 0;
    private int maxRepeats = 1;

    private bool wandering = true;
    private bool socialising = false;
    private bool idlingTogether = false;

    private Villager socialTarget;       // villager being approached
    private VillagerAI socialTargetAI;

    public WanderState(VillagerAI villager) :base (villager) 
    {
        rate = -0.001f;
    }

    public override void Enter()
    {
        VillageData.Instance.AddWanderingVillager(villager.villagerData);

        villager.agent.enabled = true;

        if (repeatCount == 0)
            maxRepeats = Random.Range(1, 11);

        StartWander();
    }

    protected override void OnExecute()
    {
        if (socialising)
        {
            // Move toward social target
            if (!villager.agent.pathPending &&
                villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, villager.reachThreshold))
            {
                // Reached target → idle together
                StartIdleTogether();
            }
        }
        else if (idlingTogether)
        {
            // Both villager and target idle
            timer += Time.deltaTime;
            if (timer >= idleTime)
            {
                EndIdleTogether();
            }
        }
        else if (wandering)
        {
            // Normal wandering
            if (!villager.agent.pathPending &&
                villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, villager.reachThreshold))
            {
                StartIdle();
            }
        }
        else
        {
            // Regular idle
            timer += Time.deltaTime;
            if (timer >= idleTime)
            {
                PickNextState();
            }
        }
    }

    private void StartWander()
    {
        wandering = true;
        socialising = false;
        idlingTogether = false;
        socialTarget = null;
        socialTargetAI = null;

        // Try to pick a wandering villager to socialise with
        socialTarget = VillageData.Instance.GetWanderingVillager(exclude: villager.villagerData);
        if (socialTarget != null)
        {
            socialTargetAI = socialTarget.GetComponent<VillagerAI>();
            if (socialTargetAI != null)
            {
                socialising = true;
                wandering = false;
                socialTargetAI.agent.isStopped = true; // target stops moving
                target = socialTarget.transform.position;
                villager.agent.SetDestination(target);
                if (villager.animator != null)
                    villager.animator.SetBool(villager.moveBool, true);

                Debug.Log(villager.name + " is going to socialise with " + socialTarget.name);
                villager.villagerData.socialisedRecently = true;
                return;
            }
        }

        // No social target → wander randomly
        if (villager.TryGetRandomNavMeshPoint(villager.homePosition, villager.wanderRadius, out target))
        {
            wandering = true;
            villager.agent.SetDestination(target);
            if (villager.animator != null)
                villager.animator.SetBool(villager.moveBool, true);
        }
    }

    private void StartIdleTogether()
    {
        socialising = false;
        idlingTogether = true;
        timer = 0f;
        idleTime = Random.Range(villager.minIdleTime, villager.maxIdleTime);

        // Stop both villager and target
        villager.agent.isStopped = true;
        if (villager.animator != null)
            villager.animator.SetBool(villager.moveBool, false);

        if (socialTargetAI != null)
        {
            socialTargetAI.agent.isStopped = true;
            if (socialTargetAI.animator != null)
                socialTargetAI.animator.SetBool(villager.moveBool, false);
        }

        Debug.Log(villager.name + " and " + socialTarget.name + " are idling together for " + idleTime + " seconds");
    }

    private void EndIdleTogether()
    {
        idlingTogether = false;

        // Resume wandering for target
        if (socialTargetAI != null)
            socialTargetAI.agent.isStopped = false;

        // Pick next state for this villager
        PickNextState();
    }

    private void StartIdle()
    {
        wandering = false;
        timer = 0f;
        idleTime = Random.Range(villager.minIdleTime, villager.maxIdleTime);

        if (villager.animator != null)
            villager.animator.SetBool(villager.moveBool, false);
    }

    private void PickNextState()
    {
        repeatCount++;
        if (repeatCount < maxRepeats)
        {
            StartWander();
        }
        else
        {
            villager.SetRole(villager.villagerData.GetRandomRole());
        }
    }

    public override void OnExit()
    {
        VillageData.Instance.RemoveWanderingVillager(villager.villagerData);

        villager.agent.ResetPath();
        if (villager.animator != null)
            villager.animator.SetBool(villager.moveBool, false);
        villager.agent.enabled = true;
        villager.agent.isStopped = false;
    }

}