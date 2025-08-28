using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Villager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugSetSick;

    private bool _isSick;

    // Event fired when sickness changes to true
    public event Action BecameSick;

    public bool isSick
    {
        get => _isSick;
        set
        {
            if (!_isSick && value) // only trigger when changing from false → true
                BecameSick?.Invoke();

            _isSick = value;
        }
    }

    public string gatherType = "food";
    public Villager_Role role;

    public float hunger = 100f;
    public float hungerRate = 1f;

    public float health = 100f;

    public float speedMultiplier = 1f;

    public float sickModifier = 1f;

    public bool isDead = false;

    public float workChanceMultiplier = 1f;

    private void Update()
    {
        if (debugSetSick != isSick)
            isSick = debugSetSick;
    }

    void Start()
    {
        var agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

    }

    public void IncrementFood(float incrementAmount)
    {
        hunger += incrementAmount;
        if (hunger > 100)
            hunger = 100;
    }

    public void IncrementHealth(float incrementAmount)
    {
        health += (incrementAmount * sickModifier);
        if (health > 100)
            health = 100f;

        if(health > 50)
        {
            isSick = false;
        }

        if (health < 20)
        {
            isSick = true;
        }

        if (health < 0)
        {
            health = 0;
            isDead = true;
        }
    }

    #region Skills
    public Dictionary<VillagerSkills, float> skills;
    public Dictionary<VillagerSkills, float> learnModifiers;


    [Header("Starting skills: Order of Research, build, gather, heal")]
    public int[] startingSkills = new int[4] { 1, 1, 1, 1 };

    [Header("Affects how quickly villager learns skills in order: research, build, gather, heal")]
    public int[] skillLearnModifier = new int[4] { 1, 1, 1, 1 };

    private void Awake()
    {
        // Initialize default skill values (could be random, balanced, or set in inspector)
        skills = new Dictionary<VillagerSkills, float>
        {
            { VillagerSkills.Research, startingSkills[0]},
            { VillagerSkills.Build, startingSkills[1] },
            { VillagerSkills.Gather, startingSkills[2] },
            { VillagerSkills.Heal, startingSkills[3] }
        };

        learnModifiers = new Dictionary<VillagerSkills, float>
        {
            { VillagerSkills.Research, skillLearnModifier[0]},
            { VillagerSkills.Build, skillLearnModifier[1] },
            { VillagerSkills.Gather, skillLearnModifier[2] },
            { VillagerSkills.Heal, skillLearnModifier[3] }
        };
    }

    public float GetSkill(VillagerSkills type)
    {
        return skills[type];
    }

    public void SetSkill(VillagerSkills type, float value)
    {
        skills[type] = value;
    }

    public void AddSkill(VillagerSkills type)
    {
        skills[type] += (0.1f * learnModifiers[type]);
    }

    public VillagerSkills GetRandomSkill()
    {
        // Step 1: Compute total weight (sum of skill values)
        float totalWeight = 0f;
        foreach (var kv in skills)
            totalWeight += SquashSkill(kv.Value); // squash skill if needed

        // Step 2: Roll a random number between 0 and totalWeight
        float roll = UnityEngine.Random.Range(0f, totalWeight);

        // Step 3: Walk through skills until roll falls into a bucket
        foreach (var kv in skills)
        {
            roll -= SquashSkill(kv.Value);
            if (roll <= 0)
                return kv.Key;
        }

        // fallback in case of float precision issues
        return VillagerSkills.Heal;
    }

    public Villager_Role GetRandomRole()
    {
        // Step 1: Base wander weight (villagers will always have some chance to wander)
        float baseWanderWeight = 1f;

        // Step 2: Compute average skill level
        float totalSkill = 0f;
        foreach (var kv in skills)
            totalSkill += kv.Value;

        float avgSkill = totalSkill / skills.Count;

        // Step 3: Scale wander chance inversely with avgSkill
        // Higher skill → smaller wanderWeight
        // Lower skill → bigger wanderWeight
        float wanderWeight = baseWanderWeight + (10f / (1f + avgSkill));
        // tweak the 10f to control influence of skill on laziness

        // Step 4: Compute total weight
        float totalWeight = wanderWeight;
        foreach (var kv in skills)
            totalWeight += SquashSkill(kv.Value);

        // Step 5: Roll
        float roll = UnityEngine.Random.Range(0f, totalWeight);

        // Step 6: Wander check first
        if (roll < wanderWeight)
            return Villager_Role.Wander;

        roll -= wanderWeight;

        // Step 7: Pick a skill-based role
        foreach (var kv in skills)
        {
            roll -= SquashSkill(kv.Value);
            if (roll <= 0)
            {
                switch (kv.Key)
                {
                    case VillagerSkills.Research:
                        return Villager_Role.Research;
                    case VillagerSkills.Build:
                        return Villager_Role.Build;
                    case VillagerSkills.Gather:
                        return Villager_Role.Gather;
                    case VillagerSkills.Heal:
                        return Villager_Role.Heal;
                }
            }
        }

        // fallback
        return Villager_Role.Wander;

    }

    private float SquashSkill(float skill)
    {
        // Linear (no squash, direct weights):
        // return skill;

        // Logarithmic: higher values grow slower
        return Mathf.Log(skill + 1, 2);

        // Square root: diminishing but smoother than log
        // return Mathf.Sqrt(skill);

        // Normalized cap: hard limit at 1.0
        // return Mathf.Clamp01(skill / 10f);
    }
    #endregion
}
