using UnityEngine;

public class BuildObj : MonoBehaviour
{
    [Header("Build requirements")]
    public float buildTime = 20f;
    public float currentBuildProgress;
    public float maxBuildProgress;
    public bool isComplete = false;

    public void ConstructBuilding(float resourceAmount)
    {
        currentBuildProgress += resourceAmount;

        if (currentBuildProgress == maxBuildProgress)
        {
            Debug.Log("Yay! You've built the house!");
            isComplete = true;
        }
    }

}
