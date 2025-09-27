using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BuildState : VillagerStateBase
{
    private ResourceObj targetNode;
    private BuildObj building;

    private GameObject spawnedResource;
    private Coroutine gatherRoutine;
    private Coroutine buildRoutine;

    private Transform dropOffLocation;
    private bool isDelivering = false;

    private float gatherAmount;
    private float gatherTime;

    private float buildTime;

    public float pushGap = 0.04f;
    public float approachThreshold = 0.12f;
    public float spawnDistance = 1f;
    public float deliveryDistance = 0.5f;

    private bool objectNotDestroyedUnexpectedly = false;


    private enum PushState { Approaching, Pushing }
    private PushState pushState = PushState.Approaching;

    private Transform currentMoveLocation;

    public BuildState(VillagerAI villager) : base(villager) 
    {
        rate = -Mathf.Clamp(Mathf.Pow(0.5f, (villager.villagerData.GetSkill(VillagerSkills.Build) - 1) / 4f), 0.01f, 1f);
        skillType = VillagerSkills.Build;
        float skillLevel = villager.villagerData.GetSkill(skillType);
        levelUpRate = Mathf.Clamp(Mathf.Pow(0.5f, skillLevel / 5f), 0.001f, 0.1f);

    }

    public override void Enter()
    {
        if (VillageData.Instance.lumberStores == null)
            Debug.LogWarning("No wood stores assigned!");

        if (VillageData.Instance.currentBuilding == null)
            VillageData.Instance.SetCurrentBuilding();

        targetNode = VillageData.Instance.lumberStores;
        building = VillageData.Instance.currentBuilding;

        StartBuilding();
    }


    public void StartBuilding()
    {
        StartNextTask();
    }

    public void StartNextTask()
    {
        if(buildRoutine != null)
        {
            villager.StopCoroutine(buildRoutine);
            buildRoutine = null;
        }
        if (targetNode == null) return;

        dropOffLocation = building.gameObject.transform;
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
        else if (isDelivering && spawnedResource == null && !objectNotDestroyedUnexpectedly)
        {
            Debug.Log($"[GatherState] Resource destroyed unexpectedly. Restarting gather.");
            isDelivering = false;
            pushState = PushState.Approaching;
            StartNextTask();
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
        spawnedResource.GetComponent<ResourceObjData>().Init("lumber", gatherAmount, villager.gameObject, true);

        isDelivering = true;
        pushState = PushState.Approaching;
        villager.agent.isStopped = false;
    }

    private IEnumerator BuildRoutine()
    {
        Vector2 nodePos = building.transform.position;

        // Move to building first
        while (Vector2.Distance(villager.transform.position, nodePos) > 0.05f)
        {
            MoveTowards(nodePos, moveSpeed);  // uses current Rigidbody2D position each frame
            yield return null;                // wait until next frame
        }

        // Stop at the building
        villager.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        // Start building
        (float timeMult, float amountMult) = GetSkillImpact();
        buildTime = building.buildTime * timeMult * MoodEffects.GetEffects(villager.villagerData.mood).workSpeedMultiplier;

        yield return new WaitForSeconds(buildTime);

        villager.villagerData.completedTaskRecently = true;
        building.ConstructBuilding(gatherAmount);

        StartNextTask();
    }

    public override void OnResourceDelivered()
    {
        objectNotDestroyedUnexpectedly = true;
        //Move to random spot near building
        spawnedResource = null;
        buildRoutine = villager.StartCoroutine(BuildRoutine());
    }

    public void GatherResource()
    {

        (float timeMult, float amountMult) = GetSkillImpact();
        gatherAmount = targetNode.gatherAmount * amountMult * MoodEffects.GetEffects(villager.villagerData.mood).workEfficiencyMultiplier;
        gatherTime = targetNode.gatherTime * timeMult * MoodEffects.GetEffects(villager.villagerData.mood).workSpeedMultiplier;

        targetNode.GatherResource(gatherAmount);
    }

    private float ApproxColliderRadius(GameObject go)
    {
        var col = go.GetComponent<Collider2D>();
        if (col == null) return 0.5f;
        return Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);
    }
}
