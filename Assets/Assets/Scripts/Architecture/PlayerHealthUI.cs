using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour, IHealthUI
{
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TextMeshProUGUI _healthText;
    [SerializeField] private GameObject _defeatMenu;

    public void Initialize(int maxHealth)
    {
        if (_healthSlider != null)
        {
            _healthSlider.maxValue = maxHealth;
            _healthSlider.value = maxHealth;
        }
        UpdateHealthText(maxHealth);

        if (_defeatMenu != null)
            _defeatMenu.SetActive(false);

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

    public void ShowDeathMenu()
    {
        if (_defeatMenu != null)
        {
            _defeatMenu.SetActive(true);
        }
    }

    private void UpdateHealthText(int health)
    {
        if (_healthText != null)
            _healthText.text = health.ToString();
    }
}