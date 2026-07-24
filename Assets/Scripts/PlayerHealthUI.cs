using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Slider healthSlider;

    void Start()
    {
        if (healthSlider != null && playerHealth != null)
        {
            healthSlider.maxValue = playerHealth.MaxHealth;
            healthSlider.value = playerHealth.CurrentHealth;
        }
    }

    void Update()
    {
        if (healthSlider != null && playerHealth != null)
        {
            healthSlider.value = playerHealth.CurrentHealth;
        }
    }
}