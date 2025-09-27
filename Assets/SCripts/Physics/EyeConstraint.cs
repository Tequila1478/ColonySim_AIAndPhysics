using UnityEngine;

public class EyeConstraint : MonoBehaviour
{
    public Transform eyeCenter;   
    public float eyeRadius = 0.2f; 

    void LateUpdate()
    {
        Vector2 offset = transform.position - eyeCenter.position;
        if (offset.magnitude > eyeRadius)
        {
            transform.position = (Vector2)eyeCenter.position + offset.normalized * eyeRadius;
        }
    }
}
