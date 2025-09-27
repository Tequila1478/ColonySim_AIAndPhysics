using UnityEngine;
using UnityEngine.AI;

public class DestroyObjOnNavMeshLeave : MonoBehaviour
{
    private float checkInterval = 1f;

    void Start()
    {
        InvokeRepeating(nameof(CheckNavMesh), checkInterval, checkInterval);
    }

    void CheckNavMesh()
    {
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(transform.position, out hit, 0.5f, NavMesh.AllAreas))
        {
            Debug.Log($"{name} left NavMesh, destroying");
            Destroy(gameObject);
        }
    }
}
