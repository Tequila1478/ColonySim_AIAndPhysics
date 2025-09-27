using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ResearchState : VillagerStateBase
{
    public ResearchObj table;
    public Transform dropOffLocation;

    private GameObject spawnedResearch;
    private bool isDelivering = false;
    private Coroutine researchRoutine;

    private float researchAmount;
    private float researchTime;


    private enum PushState { Approaching, Pushing }
    private PushState pushState = PushState.Approaching;

    // Tuning parameters
    public float pushGap = 0.04f;
    public float approachThreshold = 0.12f;
    public float spawnDistance = 0.5f; // distance from table
    public float deliveryDistance = 0.5f; // distance to drop-off

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
            villager.SetRole(villager.villagerData.GetRandomRole());
            return;
        }

        table = VillageData.Instance.researchTable;
        table.InitialiseTable();
        dropOffLocation = VillageData.Instance.ResearchDropOffLocation.transform;

        if (VillageData.Instance.looseResearch.Count > 0)
        {
            // Pick the first available loose resource
            spawnedResearch = VillageData.Instance.looseResearch[0].gameObject;
            spawnedResearch.GetComponent<ResourceObjData>().UpdateOwnerState(villager.gameObject);

            var resourceData = spawnedResearch.GetComponent<ResourceObjData>();
           
            isDelivering = true;
            pushState = PushState.Approaching;

            return;
        }

        StartNextResearch();
    }

    public void StartNextResearch()
    {        
        isDelivering = false;
        spawnedResearch = null;
        pushState = PushState.Approaching;
    }

    protected override void OnExecute()
    {
        // Go to resource point and start gathering
        if (!isDelivering)
        {
            Vector3 tableLocation = table.GetRandomPosAtTable();
            MoveTowards(table.GetRandomPosAtTable(), moveSpeed);
            float distToNode = Vector2.Distance(villager.transform.position, tableLocation);

            Debug.DrawLine(villager.transform.position, tableLocation, Color.yellow); // villager → resource

            if (distToNode < villager.reachThreshold)
            {
                if (researchRoutine == null)
                {
                    Debug.Log($"[ResearchState] Reached resource. Starting research coroutine.");
                    researchRoutine = villager.StartCoroutine(ResearchRoutine());
                }
            }
            return;
        }

        if (isDelivering && spawnedResearch != null)
        {
            if (researchRoutine != null)
            {
                Debug.Log($"[GatherState] Stopping gather routine (delivery phase).");
                villager.StopCoroutine(researchRoutine);
                researchRoutine = null;
            }

            Vector2 resourcePos = spawnedResearch.transform.position;
            Vector2 dropPos = dropOffLocation.position;
            Vector2 dirToDrop = (dropPos - resourcePos).normalized;
            float villagerRadius = ApproxColliderRadius(villager.gameObject);
            float resourceRadius = ApproxColliderRadius(spawnedResearch.gameObject);
            Vector2 pushPos = resourcePos - dirToDrop * (villagerRadius + resourceRadius + pushGap);
            Vector2 villagerPos = villager.transform.position;

            // Visual debugging: path to drop-off and push target
            Debug.DrawLine(resourcePos, dropPos, Color.blue); // resource → drop-off
            Debug.DrawRay(dropPos, Vector2.up * 0.3f, Color.magenta); // drop-off marker
            Debug.DrawLine(villagerPos, pushPos, Color.cyan); // villager → pushPos


            switch (pushState)
            {
                case PushState.Approaching:
                    MoveTowards(pushPos, villager.agent.speed);
                    if (Vector2.Distance(villagerPos, pushPos) <= approachThreshold)
                        pushState = PushState.Pushing;
                    break;

                case PushState.Pushing:
                    Rigidbody2D villagerRb = villager.GetComponent<Rigidbody2D>();
                    if (villagerRb != null)
                        villagerRb.linearVelocity = dirToDrop * villager.agent.speed;

                    Vector2 villagerToResource = (resourcePos - villagerPos).normalized;
                    float alignment = Vector2.Dot(villagerToResource, dirToDrop);
                    if (alignment < 0.99f)
                    {
                        pushState = PushState.Approaching;
                        if (villagerRb != null) villagerRb.linearVelocity = Vector2.zero;
                    }
                    break;
            }
        }

        else if (isDelivering && spawnedResearch == null)
        {
            isDelivering = false;
            pushState = PushState.Approaching;
            StartNextResearch();
            return;
        }
    }

    public override void OnExit()
    {
        var rb = villager.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (spawnedResearch != null)
        {
            GameObject.Destroy(spawnedResearch.gameObject);
            spawnedResearch = null;
        }
    }


    public override void OnResourceDelivered()
    {
        Debug.Log($"[ResearchState] Resource delivered successfully.");
        spawnedResearch = null;
        villager.villagerData.completedTaskRecently = true;
        StartNextResearch();
    }

    private IEnumerator ResearchRoutine()
    {
        GatherResource();
        yield return new WaitForSeconds(researchTime);

        // Calculate random spawn offset outside the node's collider
        float nodeRadius = ApproxColliderRadius(table.gameObject);
        float spawnDistanceFromNode = nodeRadius + 0.5f; // small extra gap
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnDistanceFromNode;

        Vector2 spawnPosition = villager.transform.position;

        spawnedResearch = GameObject.Instantiate(table.researchPrefab, spawnPosition, Quaternion.identity);
        spawnedResearch.GetComponent<ResourceObjData>().Init("research", researchAmount, villager.gameObject);

        isDelivering = true;
        pushState = PushState.Approaching;
        villager.agent.isStopped = false;
    }

    public void GatherResource()
    {

        (float timeMult, float amountMult) = GetSkillImpact();
        researchAmount = table.researchAmount * amountMult * MoodEffects.GetEffects(villager.villagerData.mood).workEfficiencyMultiplier;
        researchTime = table.researchTime * timeMult * MoodEffects.GetEffects(villager.villagerData.mood).workSpeedMultiplier;

    }


    private float ApproxColliderRadius(GameObject go)
    {
        var col = go.GetComponent<Collider2D>();
        if (col == null) return 0.5f;
        return Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);
    }

}