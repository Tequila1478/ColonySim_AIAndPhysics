using UnityEngine;

public class GatherObj : MonoBehaviour
{
    [Header("Gather Settings")]
    public float gatherTime = 3f;
    public float gatherAmount = 10f;
    public float maxAmount = 100f;
    public float currentAmount = 100f;

    public float GatherResource(float resourceAmount)
    {
        if(currentAmount - resourceAmount > 0)
        return resourceAmount;

        else
        {
            return currentAmount;
        }
    }

    public void incrementResource(float resourceAmount)
    {
        currentAmount += resourceAmount;
    }
}
