using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class VillageData : MonoBehaviour
{       

    public static VillageData Instance { get; private set; }

    [Header("Resources")]
    public float villagerCount;
    public float foodCount;
    public float lumberCount;
    public float researchCount;

    [Header("Resource Drop off locations")]
    public ResourceObj lumberStores;
    public ResourceObj foodStores;
    public Transform hospitalLocation;
    public ResourceObj ResearchDropOffLocation;

    [Header("All Resource Locations")]
    public List<GatherObj> gatherFoodPoints = new List<GatherObj>();
    public List<GatherObj> gatherLumberPoints = new List<GatherObj>();
    public List<BuildObj> buildings = new List<BuildObj>();
    public ResearchObj researchTable;

    [Header("Active Construction Project")]
    public BuildObj currentBuilding;

    [Header("Healing Settings")]
    public float healTime = 20f;
    public float healAmount = 20f;

    public Dictionary<Villager, float> sickVillagers;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate VillageData found, destroying " + gameObject.name);
            Destroy(gameObject); // enforce singleton
            return;
        }
        Instance = this;
        Debug.Log("VillageData instance set on: " + gameObject.name);

        GetNumberOfVillagers();
    }

    public void GetNumberOfVillagers()
    {
        //Get every villager
    }

    public void IncrementFood(float foodAmount)
    {
        foodCount += foodAmount;
    }

    public void IncrementResearch(float researchAmount)
    {
        researchCount += researchAmount;
        Debug.Log("Research updated!: " + researchAmount);
    }

    public void IncrementLumber(float lumberAmount)
    {
        lumberCount += lumberAmount;
    }

    #region Gather Data

    public Transform GetDropOffLocation(string type)
    {
        if (type == "food")
        {
            return foodStores.transform;
        }
        else if (type == "lumber")
        {
            return lumberStores.transform;
        }
        else
        {
            Debug.Log("incorrect gather type");
            return null;
        }
    }

    public float CheckGatherPoints(string type)
    {
        if (type == "food")
        {
            return gatherFoodPoints.Count;
        }
        else if (type == "lumber")
        {
            return gatherLumberPoints.Count;
        }
        else
        {
            Debug.Log("incorrect gather type");
            return 0;
        }
    }

    public GatherObj GetRandomGatherPoint(string type)
    {
        if (type == "food")
        {
            if (gatherFoodPoints.Count == 0) return null;
            int index = Random.Range(0, gatherFoodPoints.Count);
            return gatherFoodPoints[index];
        }
        else if (type == "lumber")
        {
            if (gatherLumberPoints.Count == 0) return null;
            int index = Random.Range(0, gatherLumberPoints.Count);
            return gatherLumberPoints[index];
        }
        else
        {
            Debug.Log("incorrect gather type");
            return null;
        }
    }
    #endregion

    #region BuildData

    public BuildObj GetCurrentBuilding()
    {
        if (currentBuilding)
            return currentBuilding;
        else //find a random building
        {
            if (buildings.Count == 0) return null;
            int index = Random.Range(0, buildings.Count);
            return buildings[index];
        }
    }

    public void SetCurrentBuilding()
    {
        currentBuilding = GetCurrentBuilding();
    }

    #endregion

    #region ResearchData

    #endregion

    #region SkillCurveSettings
    [SerializeField] private AnimationCurve skillCurve;


    public float GetSkillEffect(float skillLevel)
    {
        return skillCurve.Evaluate(skillLevel); // curve maps skill → effectiveness
    }
    #endregion

    #region healing
    public void AddSickVillager(Villager sickVillager)
    {
        sickVillagers.Add(sickVillager, sickVillager.health);
    }

    public void RemoveSickVillager(Villager healedVillager)
    {
        sickVillagers.Remove(healedVillager);
    }

    public Villager GetSickVillager()
    {
        if(sickVillagers.Count == 0)
        {
            return null;
        }
        //Returns the sickest villager
        var sortedByValue = sickVillagers.OrderBy(pair => pair.Value);
        return sortedByValue.First().Key;

    }

    #endregion

    #region UI Stats

    public TMP_Text researchTextBox;
    public TMP_Text foodTextBox;
    public TMP_Text woodTextBox;
    private void Update()
    {
        researchTextBox.text = researchCount.ToString();
        foodTextBox.text = foodCount.ToString();
        woodTextBox.text = lumberCount.ToString();

    }

    #endregion
}
