using UnityEngine;
using UnityEngine.AI;

public class ResearchState : VillagerStateBase
{
    public ResearchObj table;
    public ResourceObj researchDropOffLocation;

    private IVillagerSubState currentSubState;

    public float researchCarried;

    public ResearchState(VillagerAI villager) : base(villager) 
    {
        rate = -Mathf.Clamp(Mathf.Pow(0.5f, (villager.villagerData.GetSkill(VillagerSkills.Research) - 1) / 4f), 0.01f, 1f);
        skillType = VillagerSkills.Research;
        float skillLevel = villager.villagerData.GetSkill(skillType);
        levelUpRate = Mathf.Clamp(Mathf.Pow(0.5f, skillLevel / 5f), 0.0001f, 0.1f);

    }


    public override void Enter()
    {
        if (VillageData.Instance.researchTable == null || VillageData.Instance.ResearchDropOffLocation == null)
        {
            Debug.LogWarning("ResearchState: table or dropOffLocation not assigned.");
            return;
        }

        table = VillageData.Instance.researchTable;
        table.InitialiseTable();
        researchDropOffLocation = VillageData.Instance.ResearchDropOffLocation;
        Debug.Log(researchDropOffLocation);

        StartMoveTo(table.gameObject);
    }

    protected override void OnExecute()
    {
        currentSubState?.Execute();
    }

    public override void Exit()
    {
        currentSubState?.Exit();
    }
    public void StartMoveTo(GameObject location)
    {
        SwitchSubState(new Research_MoveState(this, villager, location));
    }
    public void SwitchSubState(IVillagerSubState newState)
    {
        currentSubState?.Exit();
        currentSubState = newState;
        currentSubState.Enter();
    }

    // Helpers for sub-states
    public void StartResearching()
    {
        SwitchSubState(new Research_ResearchingState(this, villager, table));
    }

    public void StartDelivering()
    {
        SwitchSubState(new Research_DeliverResearchState(this, villager, researchDropOffLocation, ref researchCarried));
    }

    public override void OnDropped()
    {
        currentSubState?.Exit();
        villager.SetRole(villager.villagerData.GetRandomRole());
    }

}


/*
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ResearchState : IVillagerState
{
    private VillagerAI villager;

    private ResearchObj table;
    private ResourceObj researchDropOffLocation;

    private bool atTable = false;
    private bool carryingResearch = false;
    private float researchCarried;

    private Vector3 tableTargetPosition;
    private Vector3 currentMoveLocation;


    public ResearchState(VillagerAI villager)
    {
        this.villager = villager;
    }

    public void Enter()
    {
       if (VillageData.Instance.researchTable == null || VillageData.Instance.ResearchDropOffLocation == null)
       {
           Debug.LogWarning("ResearchState: table or dropOffLocation not assigned.");
           return;
       }
            table = VillageData.Instance.researchTable;
            table.InitialiseTable();
            researchDropOffLocation = VillageData.Instance.ResearchDropOffLocation;

            StartResearching();
    }

    public void StartResearching()
    {
        StartNextResearch();
    }

    private void StartNextResearch()
    {
        // pick random X along table bounds
        float randomX = Random.Range(table.tableMinX.x, table.tableMaxX.x);
        // set Y to table's minimum Y, Z stays the same
        float yPos = table.tableMinX.y; // always use bounds.min.y
        tableTargetPosition = new Vector3(randomX, yPos, table.tableMinX.z);

        currentMoveLocation = tableTargetPosition;
        atTable = false;

        MoveTo(currentMoveLocation);
    }

    private IEnumerator ResearchAtTable()
    {
        atTable = true;
        villager.agent.isStopped = true; // pause agent while researching
        // play research animation here
        yield return new WaitForSeconds(table.researchTime);
        if (!carryingResearch)
        {
            carryingResearch = true;
            researchCarried = table.GatherResource(villager);
        }
        // move to store
        currentMoveLocation = researchDropOffLocation.transform.position;
        villager.agent.isStopped = false;
        MoveTo(currentMoveLocation);
        yield break;
    }

    private IEnumerator DeliverResearch()
    {
        villager.agent.isStopped = true;
        yield return new WaitForSeconds(researchDropOffLocation.gatherTime);

        if (carryingResearch)
        {
            carryingResearch = false;
            VillageData.Instance.IncrementResearch(researchCarried);
            researchCarried = 0;
        }
        villager.agent.isStopped = false;
        //Loop back to table
        StartNextResearch();

        yield break;
    }


    public void Execute()
    {
       if (villager.agent.pathPending) return;

       // arrived at table target -> start researching
       if (!atTable && !carryingResearch &&
           villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, villager.reachThreshold))
       {
           villager.agent.isStopped = true;
           villager.StartCoroutine(ResearchAtTable());
       }

       // arrived at store -> deliver and loop
       if (carryingResearch &&
           villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, villager.reachThreshold))
       {
           villager.StartCoroutine(DeliverResearch());
       }
    }

public void Exit()
{
   // make sure agent is moving if we exit mid-action
   villager.agent.isStopped = false;
}


private void MoveTo(Vector3 pos)
{
   if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
   {
       villager.agent.SetDestination(hit.position);
       if (villager.animator != null) villager.animator.SetBool(villager.moveBool, true);
   }
   else
   {
       Debug.LogWarning("ResearchState.MoveTo: target not on NavMesh, using table center.");
       villager.agent.SetDestination(table.transform.position);
   }
}

}
*/