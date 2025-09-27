using UnityEngine;

public class FollowVillager : MonoBehaviour
{
    public Transform villager;        
    public Vector3 offset = new Vector3(0, 1.5f, 0);
    private void Start()
    {
        if (villager == null)
            villager = transform.parent;

        offset = transform.position - villager.position;
    }

    void LateUpdate()
    {
        if (villager == null) return;

        // Keep the offset relative to villager position
        transform.position = villager.position + offset;

        // Reset rotation so it doesn't spin with the villager
        transform.rotation = Quaternion.identity;
    }

}
