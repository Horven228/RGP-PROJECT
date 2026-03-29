using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour, IHealthUI
{
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TextMeshProUGUI _healthText;
    

    public void Initialize(int maxHealth)
    {
        if (_healthSlider != null)
        {
            _healthSlider.maxValue = maxHealth;
            _healthSlider.value = maxHealth;
        }
        UpdateHealthText(maxHealth);

    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (_healthSlider != null)
            _healthSlider.value = currentHealth;

        UpdateHealthText(currentHealth);
    }

    public void HideHealthBar()
    {
        if (_healthSlider != null)
            _healthSlider.gameObject.SetActive(false);


    }


    private void UpdateHealthText(int health)
    {
        if (_healthText != null)
            _healthText.text = health.ToString();
    }
}