using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Здоровье")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Эффекты")]
    [SerializeField] private GameObject _damageEffectPrefab;

    [Header("Шкала здоровья")]
    [SerializeField] private Slider _healthSlider;

    [Header("Текст здоровья")]
    [SerializeField] private TextMeshProUGUI _healthText;

    [Header("Меню поражения")]
    [SerializeField] private GameObject _defeatMenu;

    [Header("Компоненты игрока")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerInput _playerInput;

    [Header("Анимация смерти")]
    [SerializeField] private Animator _playerAnimator;
    [SerializeField] private string _deathBoolName = "Death";
    [SerializeField] private float _deathAnimationDelay = 1.5f; // Задержка перед показом меню

    private bool isDead = false;

    // Реализация интерфейса IDamageable
    public bool IsDead => isDead;
    public Transform Transform => transform;

    void Start()
    {
        currentHealth = maxHealth;

        if (_healthSlider != null)
        {
            _healthSlider.maxValue = maxHealth;
            _healthSlider.value = currentHealth;
        }

        UpdateHealthText();

        if (_defeatMenu != null)
        {
            _defeatMenu.SetActive(false);
        }

        // Автоматически находим компоненты
        if (_playerController == null)
            _playerController = GetComponent<PlayerController>();

        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInput>();

        // Автоматически находим Animator
        if (_playerAnimator == null)
            _playerAnimator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (_healthSlider != null)
        {
            _healthSlider.value = currentHealth;
        }

        UpdateHealthText();

        if (_damageEffectPrefab != null)
        {
            GameObject effect = Instantiate(_damageEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthText()
    {
        if (_healthText != null)
        {
            _healthText.text = currentHealth.ToString();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Игрок умер!");

        // Отключаем управление сразу
        if (_playerController != null)
        {
            _playerController.enabled = false;
        }

        if (_playerInput != null)
        {
            _playerInput.enabled = false;
        }

        // Запускаем анимацию смерти
        if (_playerAnimator != null && !string.IsNullOrEmpty(_deathBoolName))
        {
            _playerAnimator.SetBool(_deathBoolName, true);
            Debug.Log("Запущена анимация смерти игрока");
        }

        // Вызываем метод показа меню через задержку
        Invoke(nameof(ShowDefeatMenu), _deathAnimationDelay);
    }

    private void ShowDefeatMenu()
    {
        // Разблокируем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Показываем меню
        if (_defeatMenu != null)
        {
            _defeatMenu.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}