using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public string sceneName = "SampleScene";
    public void LoadSampleScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
