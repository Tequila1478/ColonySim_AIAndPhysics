using TMPro;
using UnityEngine;

public class PauseScript : MonoBehaviour
{
    public TMP_Text buttonText;
    private bool _isPaused = false;
    private string paused = "paused";
    private string unpaused = "unpaused";
    public GameObject pauseUI;

    public bool isPaused => _isPaused;

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        pauseUI.SetActive(isPaused);

        if (isPaused)
        {
            buttonText.text = paused;
        }
        else
        {
            buttonText.text = unpaused;
        }
    }

}
