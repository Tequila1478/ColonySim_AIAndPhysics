using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Villager_Research : MonoBehaviour
{
    [Header("References")]
    public Transform researchTable;  // table GameObject
    public Transform researchStore;  // storage location

    [Header("Settings")]
    public float researchTime = 4f;       // seconds spent researching at the table
    public float reachThreshold = 0.4f;   // distance to consider “arrived”
    public bool startOnAwake = true;

    [Header("Colour")]
    public Color colour;

    [Header("Optional Animator")]
    public Animator animator;
    public string moveBool = "isMoving";

    private NavMeshAgent agent;
    private Vector3 tableMinX;
    private Vector3 tableMaxX;
    private Vector3 currentTarget;
    private bool atTable = false;

    private void Start()
    {
        StartResearching();
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (researchTable == null || researchStore == null)
        {
            Debug.LogWarning("ResearchTable or ResearchStore not assigned!");
            enabled = false;
            return;
        }

        // define table X bounds using the collider
        Collider col = researchTable.GetComponent<Collider>();
        if (col != null)
        {
            tableMinX = new Vector3(col.bounds.min.x, transform.position.y, col.bounds.center.z);
            tableMaxX = new Vector3(col.bounds.max.x, transform.position.y, col.bounds.center.z);
        }
        else
        {
            // fallback: just use table position if no collider
            tableMinX = tableMaxX = researchTable.position;
        }

        gameObject.GetComponent<SpriteRenderer>().color = colour;

    }

    void OnEnable()
    {
        if (startOnAwake)
            StartResearching();
    }

    void Update()
    {
        // animator
        if (animator != null)
        {
            bool moving = agent.velocity.sqrMagnitude > 0.01f;
            animator.SetBool(moveBool, moving);
        }

        // check if arrived
        if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, reachThreshold))
        {
            if (!atTable && currentTarget == tableTargetPosition)
            {
                StartCoroutine(ResearchAtTable());
            }
            else if (atTable && currentTarget == researchStore.position)
            {
                DeliverResearch();
            }
        }
    }

    #region Research Logic
    private Vector3 tableTargetPosition;

    private void StartNextResearch()
    {
        // pick random X along table bounds
        float randomX = Random.Range(tableMinX.x, tableMaxX.x);
        // set Y to table's minimum Y, Z stays the same
        float yPos = tableMinX.y; // always use bounds.min.y
        tableTargetPosition = new Vector3(randomX, yPos, tableMinX.z);

        currentTarget = tableTargetPosition;
        atTable = false;

        MoveTo(currentTarget);
    }

    private IEnumerator ResearchAtTable()
    {
        atTable = true;
        agent.isStopped = true; // pause agent while researching
        // optional: play research animation here
        yield return new WaitForSeconds(researchTime);

        // move to store
        currentTarget = researchStore.position;
        agent.isStopped = false;
        MoveTo(currentTarget);
    }

    private void DeliverResearch()
    {
        // optional: update inventory / UI
        // loop back to table
        StartNextResearch();
    }

    private void MoveTo(Vector3 target)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            Debug.LogWarning("Target not on NavMesh: " + target);
        }
    }

    public void StartResearching()
    {
        StartNextResearch();
    }

    public void PauseAI()
    {
        agent.isStopped = true;
    }

    public void ResumeAI()
    {
        agent.isStopped = false;
        MoveTo(currentTarget);
    }
    #endregion
}