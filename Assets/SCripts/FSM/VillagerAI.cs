using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class VillagerAI : MonoBehaviour
{
    [Header("Role Settings")]
    public Villager_Role role;
    private Villager_Role currentRole; // for detecting changes

    public Villager villagerData;

    [Header("General")]
    public bool startOnAwake = true;
    public float reachThreshold = 0.4f;

    [Header("Animator (Optional)")]
    public Animator animator;
    public string moveBool = "isMoving";

    [Header("Wander Settings")]
    public float wanderRadius = 8f;
    public float minIdleTime = 1f;
    public float maxIdleTime = 4f;
    public int sampleAttempts = 30;

    [Header("Hunger/Health Settings")]
    public float coolDownTimeOnEat = 600f;
    public bool canTryEat = true;
    public float eatCooldown = 0;

    [Header("Sickness Settings")]
    public bool isSick
    {
        get => _isSick;
        set
        {
            if (!_isSick && value)
            {
                // Trigger logic for becoming sick
                OnBecomeSick();
            }
            _isSick = value;

            // Keep villagerData in sync
            villagerData.isSick = _isSick;
        }
    }
    private bool _isSick;

    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public VillagerFSM fsm;
    [HideInInspector] public Vector3 homePosition;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        fsm = gameObject.AddComponent<VillagerFSM>();

        villagerData = GetComponent<Villager>();
        currentRole = role;

        agent.speed *= villagerData.speedMultiplier;
        agent.radius = 0.1f; // very small to avoid collisions with agents
        agent.avoidancePriority = 50; // can adjust priority if you have multiple agents

        // Keep obstacle avoidance active
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        villagerData.BecameSick += OnBecomeSick;
    }

    void OnDestroy()
    {
        villagerData.BecameSick -= OnBecomeSick;
    }

    private void OnBecomeSick()
    {
        SetRole(Villager_Role.Sick);
    }

        void Start()
    {
        if (startOnAwake)
            ApplyRole(role);
        float waitTime = 60f/villagerData.hungerRate;
        InvokeRepeating("DecreaseFood", waitTime, waitTime);
    }


    void DecreaseFood()
    {
        villagerData.hunger -= 1;
        if(villagerData.hunger < 50 && canTryEat)
        {
            SetRole(Villager_Role.Eat);
        }
        if(villagerData.hunger <= 0)
        {
            villagerData.IncrementHealth(-1f);
            if (villagerData.isDead)
            {
                SetRole(Villager_Role.Dead);
            }
        }
    }

    public bool TryGetRandomNavMeshPoint(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < sampleAttempts; i++)
        {
            Vector2 randomXZ = Random.insideUnitCircle * radius;
            Vector3 randomPoint = new Vector3(center.x + randomXZ.x, center.y + 5f, center.z + randomXZ.y);
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
    }

    #region Roles
    void Update()
    {
        // Detect if role has been changed externally
        if (role != currentRole)
        {
            currentRole = role;
            ApplyRole(role);
        }

        if (!canTryEat)
        {
            eatCooldown += Time.deltaTime;

            if (eatCooldown >= coolDownTimeOnEat)
            {
                canTryEat = true;
            }
        }

    }

    public void SetRole(Villager_Role newRole)
    {
        role = newRole;
        // role change will be caught in Update
    }

    public void ApplyRole(Villager_Role newRole)
    {
        switch (newRole)
        {
            case Villager_Role.Wander:
                fsm.ChangeState(new WanderIdleState(this));
                break;
            case Villager_Role.Sick:
                fsm.ChangeState(new WanderIdleState(this));
                break;
            case Villager_Role.Gather:
                fsm.ChangeState(new GatherState(this));
                break;
            case Villager_Role.Research:
                fsm.ChangeState(new ResearchState(this));
                break;
            case Villager_Role.Build:
                fsm.ChangeState(new BuildState(this));
                break;
            case Villager_Role.Eat:
                fsm.ChangeState(new EatState(this));
                break;
            case Villager_Role.Dead:
                fsm.ChangeState(new DeadState(this));
                break;
            case Villager_Role.Heal:
                fsm.ChangeState(new WanderIdleState(this));
                break;
            default:
                fsm.ChangeState(new WanderIdleState(this));
                break;
        }
        ChangeColour();
    }
    #endregion

    public void ChangeColour()
    {
        Color colour;
        switch (role)
        {
            case Villager_Role.Wander:
                colour = Color.white;
                break;
            case Villager_Role.Sick:
                colour = Color.red;
                break;
            case Villager_Role.Gather:
                colour = Color.green;
                break;
            case Villager_Role.Research:
                colour = Color.blue;
                break;
            case Villager_Role.Build:
                colour = Color.yellow;
                break;
            case Villager_Role.Eat:
                colour = Color.rebeccaPurple;
                break;
            case Villager_Role.Dead:
                colour = Color.darkSlateGray;
                break;
            case Villager_Role.Heal:
                colour = Color.aquamarine;
                break;
            default:
                colour = Color.black;
                break;
        }

        GetComponent<SpriteRenderer>().color = colour;
    }

    public void StopMovingForHeal()
    {

    }

    public void StartMovingAfterHeal()
    {

    }
}