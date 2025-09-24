using UnityEngine;

public class MoodEffects 
{
    public float moveSpeedMultiplier = 1f;
    public float energyConsumptionMultiplier = 1f;
    public float workEfficiencyMultiplier = 1f;
    public float wanderChanceModifier = 1f; // extra chance to switch to wander
    public float workSpeedMultiplier = 1f;

    public static MoodEffects GetEffects(Mood mood)
    {
        switch (mood)
        {
            case Mood.Sleepy:
                return new MoodEffects
                {
                    moveSpeedMultiplier = 0.7f,
                    energyConsumptionMultiplier = 0.7f,
                    workEfficiencyMultiplier = 1f,
                    wanderChanceModifier = 0.2f,
                    workSpeedMultiplier = 0.8f
                };

            case Mood.Angry:
                return new MoodEffects
                {
                    moveSpeedMultiplier = 1.2f,
                    energyConsumptionMultiplier = 1.3f, // consumes more energy
                    workEfficiencyMultiplier = 0.8f,
                    wanderChanceModifier = 0f,
                    workSpeedMultiplier = 1.2f
                };

            case Mood.Happy:
                return new MoodEffects
                {
                    moveSpeedMultiplier = 1.2f, // faster movement
                    energyConsumptionMultiplier = 1f,
                    workEfficiencyMultiplier = 1.2f,
                    wanderChanceModifier = 0f,
                    workSpeedMultiplier = 1.2f
                };

            case Mood.Sad:
                return new MoodEffects
                {
                    moveSpeedMultiplier = 0.8f,
                    energyConsumptionMultiplier = 1f,
                    workEfficiencyMultiplier = 1f,
                    wanderChanceModifier = 0.3f,
                    workSpeedMultiplier = 0.8f
                };

            case Mood.Neutral:
            default:
                return new MoodEffects(); // all default 1
        }
    }

}
