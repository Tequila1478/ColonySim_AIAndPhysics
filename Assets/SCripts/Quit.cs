using UnityEngine;

public class Quit : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Quit button pressed"); // Only visible in editor
        Application.Quit(); // Actually quits the build
    }
}
