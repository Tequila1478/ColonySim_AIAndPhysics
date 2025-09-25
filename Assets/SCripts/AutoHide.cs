using UnityEngine;

public class AutoHide : MonoBehaviour
{
    public float duration = 5f; // seconds
    private float timer = 0f;
    private bool isActive = false;

    // Call this method to start the timer
    public void OnEnable()
    {
        timer = duration;
        isActive = true;
    }

    private void Update()
    {
        if (!isActive) return;

        timer -= Time.unscaledDeltaTime;
        if (timer <= 0f)
        {
            gameObject.SetActive(false);

            isActive = false; // stop timer
        }
    }
}
