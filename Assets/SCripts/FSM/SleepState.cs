using System.Collections;
using UnityEngine;

public class SleepState : VillagerStateBase
{
    private Transform sleepLocation;
    private bool isSleeping = false;
    private Coroutine sleepRoutine;

    private float sleepTimer;
    public SleepState(VillagerAI villager) : base(villager) { }


    private BedObj setBed;

    public override void Enter()
    {
        Debug.Log("Villager sleepy");
        // 1. Find a sleep location
        sleepLocation = VillageData.Instance.GetFreeSleepLocation();

        if (sleepLocation == null)
        {
            Debug.LogWarning($"{villager.name} has no sleep location available!");
            villager.SetRole(Villager_Role.Wander); // fallback
            return;
        }

        // Claim it (VillageData or BedObject should track occupancy!)
        setBed = sleepLocation.GetComponent<BedObj>();
        if (setBed != null && setBed.IsOccupied)
        {
            Debug.Log($"{villager.name} tried to sleep but bed is occupied.");
            villager.SetRole(Villager_Role.Wander);
            return;
        }
        if (setBed != null) setBed.IsOccupied = true;

        Debug.Log("Villager moving to bed");
        // Move to bed
        villager.MoveTo(sleepLocation.position);
    }

    public override void Execute()
    {
        if (villager.agent.pathPending) return;

        if (!isSleeping && villager.agent.remainingDistance <= Mathf.Max(villager.agent.stoppingDistance, villager.reachThreshold))
        {
            // Arrived at bed → start sleeping
            villager.agent.isStopped = true;
            sleepRoutine = villager.StartCoroutine(SleepCoroutine());
        }
    }

    private IEnumerator SleepCoroutine()
    {
        isSleeping = true;
        if (villager.animator != null)
            villager.animator.SetBool("isSleeping", true);



        while (villager.villagerData.energy < 100)
        {
            villager.villagerData.energy += setBed.sleepRecoveryRate * Time.deltaTime;


            // Calculate energy % relative to max health/energy
            float energy = villager.villagerData.energy;
            float healthThreshold = villager.villagerData.health;
            float baseChance = 0f + villager.villagerData.wakeUpChance;

            float wakeChance = 0f;

            if (energy > healthThreshold)
            {
                wakeChance = baseChance;
            }

            else if (energy < 50)
            {
                wakeChance = 0;
            }

            else
            {
                float t = Mathf.InverseLerp(healthThreshold, 100f, energy); // 0 → 1 from health → 100
                wakeChance = Mathf.Lerp(baseChance, 1f, t);
            }

            float totalChance = Mathf.Clamp01(wakeChance);

            // Roll dice
            if (Random.value < totalChance * Time.deltaTime)
            {
                // Wakes up early
                break;
            }

            yield return null;
        }

        WakeUp();
    }

    private void WakeUp()
    {

        isSleeping = false;

        if (villager.animator != null)
            villager.animator.SetBool("isSleeping", false);

        // Free the bed
        if (sleepLocation != null)
        {
            BedObj bed = sleepLocation.GetComponent<BedObj>();
            if (bed != null) bed.IsOccupied = false;
        }

        villager.SetRole(Villager_Role.Wander); // go back to normal routine
    }

    public override void Exit()
    {
        if (sleepRoutine != null)
        {
            villager.StopCoroutine(sleepRoutine);
            sleepRoutine = null;
        }

        isSleeping = false;
    }

    public override void OnDropped()
    {
        WakeUp();
    }
}
