using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour, IHealthUI
{
    [SerializeField] private Slider _healthSlider;

    public void Initialize(int maxHealth)
    {
        if (_healthSlider != null)
        {
            _healthSlider.maxValue = maxHealth;
            _healthSlider.value = maxHealth;
        }
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (_healthSlider != null)
            _healthSlider.value = currentHealth;
    }

    public void HideHealthBar()
    {
        if (_healthSlider != null)
            _healthSlider.gameObject.SetActive(false);
    }

    
}