using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private void Awake()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }
    }

    public void SetMaxHealth(int health)
    {
        if (slider == null) return;

        slider.maxValue = health;
        slider.value = health;
    }

    public void SetHealth(int health)
    {
        if (slider == null) return;

        slider.value = health;
    }
}
