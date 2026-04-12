using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Отображает полоску и текст здоровья игроку

public class PlayerHealthUI : MonoBehaviour, IHealthUI
{
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TextMeshProUGUI _healthText;

    // Реализация метода Initialize из интерфейса
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