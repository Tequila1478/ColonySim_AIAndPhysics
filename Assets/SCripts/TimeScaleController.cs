using UnityEngine;
using UnityEngine.UI;

public class TimeScaleController : MonoBehaviour
{
    public Slider timeSlider;

    private void Start()
    {
        // Set initial value to current time scale
        timeSlider.value = Time.timeScale;

        // Add listener for when the slider value changes
        timeSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        // Change the game's time scale
        Time.timeScale = value;
    }
}
