using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class VillagerAI : MonoBehaviour
{
    [Header("Role Settings")]
    public Villager_Role role;
    public Villager_Role currentRole { get; private set; } // for detecting changes

    public Villager villagerData;
    public string state;

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
    public float coolDownTimeOnEat = 60f;
    public bool canTryEat = true;
    public float eatCooldown = 0;

    [Header("Sickness Settings")]
    public bool isBeingHealed = false; 

    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public VillagerFSM fsm;
    [HideInInspector] public Vector3 homePosition;



    public MoodEffects currentMoodEffects;

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
    }

    public void ApplyMoodEffects()
    {
        currentMoodEffects = MoodEffects.GetEffects(villagerData.mood);
        agent.speed = VillageData.Instance.villagerBaseSpeed * villagerData.speedMultiplier * currentMoodEffects.moveSpeedMultiplier;


    }

    public void OnBecomeSick()
    {
        SetRole(Villager_Role.Sick);
    }

        void Start()
    {
        if (startOnAwake)
            ApplyRole(role);

        if (VillageData.Instance != null)
        {
            VillageData.Instance.UpdateNumberOfVillagers();
        }
    }

   

    void DecreaseFood()
    {
        villagerData.hunger -= 1;
        if(villagerData.hunger < 50 && canTryEat && currentRole != Villager_Role.Sleep && currentRole != Villager_Role.Eat)
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
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
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
                eatCooldown = coolDownTimeOnEat;
            }
        }

        HandleHunger();

    }

    private float hungerAccumulator = 0f;
    private float hungerTickInterval = 60f;

    private void HandleHunger()
    {
        hungerTickInterval = Mathf.Clamp(60f / Mathf.Max(0.01f, villagerData.hungerRate), 0.1f, 60f); hungerAccumulator += Time.deltaTime; // deltaTime is already scaled by Time.timeScale

        if (hungerAccumulator >= hungerTickInterval)
        {
            hungerAccumulator -= hungerTickInterval;
            DecreaseFood();
        }
    }

    public void SetRole(Villager_Role newRole, bool forced = false)
    {
        if (!forced && fsm.currentState != null && !fsm.currentState.CanChangeRole)
            return;

        if (newRole == Villager_Role.Eat && !canTryEat && !forced)
            return;

        role = newRole;
        villagerData.role = newRole;

        TryChangeMood();
        // role change will be caught in Update
    }

    public static class WeightedRandom
    {
        public static T Choose<T>(Dictionary<T, float> weights)
        {
            float total = 0f;
            foreach (var w in weights.Values)
                total += w;

            float roll = Random.value * total;
            float cumulative = 0f;

            foreach (var kvp in weights)
            {
                cumulative += kvp.Value;
                if (roll <= cumulative)
                    return kvp.Key;
            }

            // fallback (shouldn’t happen if weights > 0)
            foreach (var kvp in weights)
                return kvp.Key;

            return default;
        }
    }

    private void TryChangeMood()
    {
        float chance = 0.1f; // 10% chance per role switch
        if (Random.value >= chance) return;

        Mood oldMood = villagerData.mood;

        // Build weighted mood options
        var weights = new Dictionary<Mood, float>
    {
        { Mood.Happy, 1f },
        { Mood.Sad, 1f },
        { Mood.Angry, 1f },
        { Mood.Neutral, 1f },
        { Mood.Sleepy, 1f }
    };

        // Apply contextual biases
        if (villagerData.energy < 20)
        {
            weights[Mood.Sleepy] += 3f; // much more likely to be sad
        }

        if (villagerData.failedTaskRecently) // you’d set this flag in e.g. Gather when resource empty
        {
            weights[Mood.Angry] += 3f;
            villagerData.failedTaskRecently = false; // reset
        }

        if (villagerData.completedTaskRecently) // set this in BuildState when finishing
        {
            weights[Mood.Happy] += 3f;
            villagerData.completedTaskRecently = false;
        }

        if(villagerData.sleptRecently) // set this in BuildState when finishing
        {
            weights[Mood.Sleepy] -= 3f;
            villagerData.sleptRecently = false;
        }

        if (villagerData.socialisedRecently) // set this in BuildState when finishing
        {
            weights[Mood.Sad] -= 3f;
            villagerData.socialisedRecently = false;
        }
        if (villagerData.hasEatenRecently) // set this in BuildState when finishing
        {
            weights[Mood.Happy] += 3f;
            weights[Mood.Sleepy] += 3f;
            villagerData.hasEatenRecently = false;
        }
        if (villagerData.wasPickedupRecently) // set this in BuildState when finishing
        {
            weights[Mood.Angry] += 3f;
            weights[Mood.Neutral] += 3f;
            villagerData.wasPickedupRecently = false;
        }

        // Pick new mood
        Mood newMood = WeightedRandom.Choose(weights);

        // Avoid rerolling the same mood (optional)
        if (newMood == oldMood)
        {
            // re-roll once more, but weaker
            newMood = WeightedRandom.Choose(weights);
        }

        villagerData.mood = newMood;
        Debug.Log($"{name} mood changed from {oldMood} → {newMood}");
        ApplyMoodEffects();

    }

    public void ApplyRole(Villager_Role newRole)
    {
        villagerData.role = newRole;
        VillagerStateBase newState = null;

        switch (newRole)
        {
            case Villager_Role.Wander:
                newState = new WanderState(this);
                //fsm.ChangeState(new WanderState(this));
                break;
            case Villager_Role.Sick:
                newState = new SickState(this);

                //fsm.ChangeState(new SickState(this));
                break;
            case Villager_Role.Gather:
                newState = new GatherState(this);

               // fsm.ChangeState(new GatherState(this));
                break;
            case Villager_Role.Research:
                newState = new ResearchState(this);

                //fsm.ChangeState(new ResearchState(this));
                break;
            case Villager_Role.Build:
                newState = new BuildState(this);

                //fsm.ChangeState(new BuildState(this));
                break;
            case Villager_Role.Eat:
                newState = new EatState(this);

                //fsm.ChangeState(new EatState(this));
                break;
            case Villager_Role.Dead:
                newState = new DeadState(this);

                //fsm.ChangeState(new DeadState(this));
                break;
            case Villager_Role.Heal:
                newState = new HealState(this);
                //fsm.ChangeState(new HealState(this));
                break;
            case Villager_Role.Sleep:
                newState = new SleepState(this);
                //fsm.ChangeState(new SleepState(this));
                break;
            case Villager_Role.PickedUp:
                newState = new PickupState(this);
                //fsm.ChangeState(new PickupState(this));
                break;
            default:
                newState = new WanderState(this);
                //fsm.ChangeState(new WanderState(this) );
                break;
        }
        state = newState.ToString();
        fsm.ChangeState(newState);
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
            case Villager_Role.Sleep:
                colour = Color.turquoise;
                break;
            case Villager_Role.PickedUp:
                colour = Color.orange;
                break;
            default:
                colour = Color.black;
                break;
        }

        GetComponent<SpriteRenderer>().color = colour;
    }

    public bool MoveTo(Vector3 target)
    {
        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
            return true;
        }
        else
        {
            Debug.LogWarning($"{name}: Target not on NavMesh: {target}");
            return false;
        }
    }

}