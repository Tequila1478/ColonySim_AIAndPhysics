using UnityEngine;

public class ResearchObj : MonoBehaviour, IRoleInteractable
{
    [Header("Research Settings")]
    public float researchTime = 60f;
    public float researchAmount = 10f;

    public Vector3 tableMinX;
    public Vector3 tableMaxX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float GatherResource(VillagerAI villager)
    {
        return researchAmount;
    }

    public void InitialiseTable()
    {
        // define table X bounds using the collider
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            tableMinX = new Vector3(col.bounds.min.x, transform.position.y, col.bounds.center.z);
            tableMaxX = new Vector3(col.bounds.max.x, transform.position.y, col.bounds.center.z);
        }
        else
        {
            Debug.Log("Help");// fallback: just use table position if no collider
            tableMinX = tableMaxX = transform.position;
        }
    }

    public void OnVillagerDropped(VillagerAI villager)
    {
        villager.SetRole(Villager_Role.Research);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Villager"))
        {
            VillagerAI villager = collision.gameObject.GetComponent<VillagerAI>();
            villager.SetRole(Villager_Role.Research);
        }
    }
}
