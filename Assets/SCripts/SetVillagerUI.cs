using TMPro;
using UnityEngine;

public class SetVillagerUI : MonoBehaviour
{
    public static SetVillagerUI Instance { get; private set; }

    public TMP_Text health_Txt;

    public TMP_Text healSkill_Txt;
    public TMP_Text researchSkill_Txt;
    public TMP_Text buildSkill_Txt;
    public TMP_Text gatherSkill_Txt;

    public TMP_Dropdown roleDropdown;
    public TMP_Text State_Txt;

    private Villager villager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate VillageData found, destroying " + gameObject.name);
            Destroy(gameObject); // enforce singleton
            return;
        }
        Instance = this;

        roleDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        foreach (var role in System.Enum.GetValues(typeof(Villager_Role)))
            options.Add(role.ToString());
        roleDropdown.AddOptions(options);

        // Listen to changes
        roleDropdown.onValueChanged.AddListener(OnRoleChanged);
    }

    public void setSkillText(Villager villager)
    {
        this.villager = villager;
        healSkill_Txt.text = villager.skills[VillagerSkills.Heal].ToString();
        researchSkill_Txt.text = villager.skills[VillagerSkills.Research].ToString();
        buildSkill_Txt.text = villager.skills[VillagerSkills.Build].ToString();
        gatherSkill_Txt.text = villager.skills[VillagerSkills.Gather].ToString();


        health_Txt.text = villager.health.ToString();
        State_Txt.text = villager.role.ToString();

    }

    private void OnRoleChanged(int index)
    {
        if (villager != null)
        {
            Villager_Role newRole = (Villager_Role)index;
            villager.GetComponent<VillagerAI>().SetRole(newRole); // your method that sets role and changes FSM
        }
    }


}
