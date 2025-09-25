using UnityEngine;

public class VillageUIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void Update()
    {
        if (VillageData.Instance.foodCount != VillageData.Instance.lastFoodCount)
        {
            VillageData.Instance.foodTextBox.text = VillageData.Instance.foodCount.ToString();
            VillageData.Instance.lastFoodCount = VillageData.Instance.foodCount;
        }

        if (VillageData.Instance.lumberCount != VillageData.Instance.lastLumberCount)
        {
            VillageData.Instance.woodTextBox.text = VillageData.Instance.lumberCount.ToString();
            VillageData.Instance.lastLumberCount = VillageData.Instance.lumberCount;
        }

        if (VillageData.Instance.researchCount != VillageData.Instance.lastResearchCount)
        {
            VillageData.Instance.researchTextBox.text = VillageData.Instance.researchCount.ToString();
            VillageData.Instance.lastResearchCount = VillageData.Instance.researchCount;
        }

        if (VillageData.Instance.GameOver)
        {
            VillageData.Instance.gameOverUI.SetActive(true);
        }

    }
}
