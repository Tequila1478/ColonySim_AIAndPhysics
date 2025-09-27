using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GatherState : VillagerStateBase
{
    private GatherObj targetNode;
    private GameObject spawnedResource;

    public Transform dropOffLocation;

    private bool isDelivering = false;

    private Coroutine gatherRoutine;

    private float gatherAmount;

    private float gatherTime;


    private enum PushState { Approaching, Pushing }
    private PushState pushState = PushState.Approaching;

    // tuning parameters
    public float pushGap = 0.04f;
    public float approachThreshold = 0.12f;
    public float spawnDistance = 1f;
    public float deliveryDistance = 0.5f;

    public GatherState(VillagerAI villager) : base(villager)
    {
        rate = -Mathf.Clamp(Mathf.Pow(0.5f, (villager.villagerData.GetSkill(VillagerSkills.Gather) - 1) / 4f), 0.01f, 1f);
        skillType = VillagerSkills.Gather;
        float skillLevel = villager.villagerData.GetSkill(skillType);
        levelUpRate = Mathf.Clamp(Mathf.Pow(0.5f, skillLevel / 5f), 0.0001f, 0.1f);
    }

    public override void Enter()
    {
        if (VillageData.Instance.looseResources.Count > 0)
        {
            // Pick the first available loose resource
            spawnedResource = VillageData.Instance.looseResources[0].gameObject;

            var resourceData = spawnedResource.GetComponent<ResourceObjData>();
            dropOffLocation = VillageData.Instance.GetDropOffLocation(resourceData.type);

            // Set AI to delivering state immediately
            dropOffLocation = VillageData.Instance.GetDropOffLocation(spawnedResource.GetComponent<ResourceObjData>().type);
            isDelivering = true;
            pushState = PushState.Approaching;

            return;
        }

        StartNextGather();
    }

    public void StartNextGather()
    {
        if (VillageData.Instance.CheckGatherPoints(villager.villagerData.gatherType) == 0)
        {
            villager.SetRole(villager.villagerData.GetRandomRole());
            return;
        }

        targetNode = VillageData.Instance.GetRandomGatherPoint(villager.villagerData.gatherType);
        if (targetNode == null)
        {
            villager.SetRole(villager.villagerData.GetRandomRole());
            return;
        }

        dropOffLocation = VillageData.Instance.GetDropOffLocation(villager.villagerData.gatherType);
        isDelivering = false;
        spawnedResource = null;
        pushState = PushState.Approaching;
    }

    protected override void OnExecute()
    {
        if (targetNode == null)
        {
            villager.SetRole(villager.villagerData.GetRandomRole());
            return;
        }

        // Go to resource point and start gathering
        if (!isDelivering)
        {
            Vector2 villagerPos = villager.transform.position;
            Vector2 nodePos = targetNode.transform.position;
            Vector2 dirToNode = (nodePos - villagerPos).normalized;

            float nodeRadius = ApproxColliderRadius(targetNode.gameObject);
            float villagerRadius = ApproxColliderRadius(villager.gameObject);
            Vector2 approachPoint = nodePos - dirToNode * (nodeRadius + villagerRadius + 0.05f);

            MoveTowards(approachPoint, moveSpeed);

            float distToNode = Vector2.Distance(villagerPos, approachPoint);

            Debug.DrawLine(villager.transform.position, targetNode.transform.position, Color.yellow); // villager → resource

            if (distToNode < villager.reachThreshold)
            {
                if (gatherRoutine == null)
                {
                    gatherRoutine = villager.StartCoroutine(GatherRoutine());
                }
            }
            return;
        }

        // Delivery phase
        if (isDelivering && spawnedResource != null)
        {
            if (gatherRoutine != null)
            {
                villager.StopCoroutine(gatherRoutine);
                gatherRoutine = null;
            }

            Vector2 resourcePos = spawnedResource.transform.position;
            Vector2 dropPos = dropOffLocation.position;
            Vector2 dirToDrop = (dropPos - resourcePos).normalized;
            float villagerRadius = ApproxColliderRadius(villager.gameObject);
            float resourceRadius = ApproxColliderRadius(spawnedResource);
            Vector2 pushPos = resourcePos - dirToDrop * (villagerRadius + resourceRadius + pushGap);
            Vector2 villagerPos = villager.transform.position;

            // Visual debugging: path to drop-off and push target
            Debug.DrawLine(resourcePos, dropPos, Color.blue); // resource → drop-off
            Debug.DrawRay(dropPos, Vector2.up * 0.3f, Color.magenta); // drop-off marker
            Debug.DrawLine(villagerPos, pushPos, Color.cyan); // villager → pushPos

            switch (pushState)
            {
                case PushState.Approaching:
                    MoveTowards(pushPos, moveSpeed);

                    if (Vector2.Distance(villagerPos, pushPos) <= approachThreshold)
                    {
                        pushState = PushState.Pushing;
                    }
                    break;

                case PushState.Pushing:
                    Rigidbody2D villagerRb = villager.GetComponent<Rigidbody2D>();
                    if (villagerRb != null)
                        villagerRb.linearVelocity = dirToDrop * moveSpeed;

                    Vector2 villagerToResource = (resourcePos - villagerPos).normalized;
                    float alignment = Vector2.Dot(villagerToResource, dirToDrop);

                    // Visual debugging: alignment
                    Debug.DrawLine(resourcePos, resourcePos + dirToDrop * 2f, Color.green); // intended push direction
                    Debug.DrawLine(villagerPos, villagerPos + villagerToResource * 2f, Color.red); // villager → resource


                    if (alignment < 0.99f)
                    {
                        pushState = PushState.Approaching;
                        if (villagerRb != null) villagerRb.linearVelocity = Vector2.zero;
                    }
                    break;
            }
        }
        else if (isDelivering && spawnedResource == null)
        {
            isDelivering = false;
            pushState = PushState.Approaching;
            StartNextGather();
            return;
        }
    }

    private IEnumerator GatherRoutine()
    {
        GatherResource();
        yield return new WaitForSeconds(gatherTime);


        // Calculate random spawn offset outside the node's collider
        float nodeRadius = ApproxColliderRadius(targetNode.gameObject);
        float spawnDistanceFromNode = nodeRadius + 0.5f; // small extra gap
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnDistanceFromNode;

        Vector2 spawnPosition = (Vector2)targetNode.transform.position + offset;

        spawnedResource = GameObject.Instantiate(targetNode.resourcePrefab, spawnPosition, Quaternion.identity);
        spawnedResource.GetComponent<ResourceObjData>().Init(villager.villagerData.gatherType, gatherAmount, villager.gameObject);

        isDelivering = true;
        pushState = PushState.Approaching;
        villager.agent.isStopped = false;

    }

    public void GatherResource()
    {

        (float timeMult, float amountMult) = GetSkillImpact();
        gatherAmount = targetNode.gatherAmount * amountMult * MoodEffects.GetEffects(villager.villagerData.mood).workEfficiencyMultiplier;
        gatherTime = targetNode.gatherTime * timeMult * MoodEffects.GetEffects(villager.villagerData.mood).workSpeedMultiplier;



        targetNode.GatherResource(gatherAmount);
        targetNode.incrementResource(-gatherAmount);
    }

    public override void OnResourceDelivered()
    {
        Debug.Log($"[GatherState] Resource delivered successfully.");
        spawnedResource = null;
        villager.villagerData.completedTaskRecently = true;
        StartNextGather();
    }

    public override void Exit()
    {
        var rb = villager.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if(spawnedResource != null)
        {
            spawnedResource.GetComponent<ResourceObjData>().RemoveOwner();
        }

        spawnedResource = null;
        isDelivering = false;
        pushState = PushState.Approaching;
    }

    private float ApproxColliderRadius(GameObject go)
    {
        var col = go.GetComponent<Collider2D>();
        if (col == null) return 0.5f;
        return Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);
    }
}