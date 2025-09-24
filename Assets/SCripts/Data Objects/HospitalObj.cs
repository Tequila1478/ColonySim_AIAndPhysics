using UnityEngine;

public class HospitalObj : MonoBehaviour, IRoleInteractable
{
    public float healDuration = 120;
    public float healAmount = 10;

    public void OnVillagerDropped(VillagerAI villager)
    {
        villager.SetRole(Villager_Role.Heal);
    }


}
