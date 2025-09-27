using UnityEngine;
using UnityEngine.AI;

public class SickState : VillagerStateBase
{
    private Vector3 target;
    private float idleTimer;
    private bool isIdle;

    // Configurable values
    private float wanderRadius = 2f;
    private float minIdleTime = 1f;
    private float maxIdleTime = 4f;
    private float reachThreshold = 0.4f;

    private bool isBeingHealed = false;

    public override bool CanChangeRole => !isBeingHealed;

    public SickState(VillagerAI villager) : base(villager) {
        rate = -0.0001f;
    }

    public override void Enter()
    {        
        SetNewDestination();
        VillageData.Instance.AddSickVillager(villager.villagerData);
    }

    protected override void OnExecute()
    {
        isBeingHealed = villager.isBeingHealed;
        //Debug.Log($"{villager.name} healing state: {villager.isBeingHealed}");
        if (villager.agent == null || !villager.agent.enabled) return;

        // If villager is no longer sick, exit state
        if (!villager.villagerData.isSick)
        {
            villager.SetRole(Villager_Role.Wander);
            return;
        }

        // If being healed, stop moving
        if (villager.isBeingHealed)
        {
            villager.agent.isStopped = true;
            if (villager.animator != null)
                villager.animator.SetBool("isMoving", false);
            villager.GetComponent<SpriteRenderer>().color = Color.black;
            return;
        }
        else
        {
            villager.agent.isStopped = false;
            villager.GetComponent<SpriteRenderer>().color = Color.red;
        }

        // Normal wandering logic
        if (villager.animator != null)
            villager.animator.SetBool("isMoving", villager.agent.velocity.sqrMagnitude > 0.01f);

        if (!isIdle)
        {
            if (!villager.agent.pathPending &&
                villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, reachThreshold))
            {
                idleTimer = Random.Range(minIdleTime, maxIdleTime);
                isIdle = true;
            }
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0f)
            {
                isIdle = false;
                SetNewDestination();
            }
        }
    }

    public override void OnExit()
    {
        VillageData.Instance.RemoveSickVillager(villager.villagerData);
        villager.agent.enabled = true;
        villager.agent.isStopped = false;
    }

    private void SetNewDestination()
    {
        if (VillageData.Instance.hospitalObj == null) return;
        if (villager.TryGetRandomNavMeshPoint(VillageData.Instance.hospitalObj.transform.position, 1f, out target))
        {
            villager.agent.SetDestination(target);
            if (villager.animator != null)
                villager.animator.SetBool(villager.moveBool, true);
        }
    }
}