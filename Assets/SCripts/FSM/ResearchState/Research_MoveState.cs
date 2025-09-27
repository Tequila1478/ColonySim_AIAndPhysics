using System.Collections;
using UnityEngine;

public class Research_MoveState : IVillagerSubState
{
    private ResearchState parent;
    private VillagerAI villager;
    private GameObject moveToLocation;

    private ResearchObj table;
    private ResourceObj resourceStore;

    private Vector3 targetPosition;

    public Research_MoveState(ResearchState parent, VillagerAI villager, GameObject moveLocation)
    {
        this.parent = parent;
        this.villager = villager;
        this.moveToLocation = moveLocation;

        if (moveLocation.GetComponent<ResearchObj>())
        {
            this.table = moveLocation.GetComponent<ResearchObj>();

            float randomX = Random.Range(table.tableMinX.x, table.tableMaxX.x);
            float yPos = table.tableMinX.y;
            targetPosition = new Vector3(randomX, yPos, table.tableMinX.z);
        }

        else if (moveLocation.GetComponent<ResourceObj>())
        {
            this.resourceStore = moveLocation.GetComponent<ResourceObj>();
            targetPosition = this.resourceStore.transform.position;
        }

        else
        {
            targetPosition = moveLocation.transform.position;
        }

    }

    public void Enter()
    {
        Debug.Log("Entering MoveToState: " + moveToLocation);
        villager.agent.isStopped = false;
        villager.agent.SetDestination(targetPosition);
        if (villager.animator != null) villager.animator.SetBool(villager.moveBool, true);
    }

    public void Execute()
    {
        if (villager.agent.enabled)
        {
            if (villager.agent.hasPath && !villager.agent.pathPending &&
                villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, villager.reachThreshold))
            {
               // if (table != null)
                   // parent.StartResearching();

                //else if (resourceStore != null)
                   // parent.StartDelivering();
            }
        }
    }

    public void Exit()
    {
        villager.agent.isStopped = true;
        if (villager.animator != null) villager.animator.SetBool(villager.moveBool, false);
    }
}