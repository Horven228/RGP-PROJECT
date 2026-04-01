using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Здоровье")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Эффекты")]
    [SerializeField] private GameObject _damageEffectPrefab;

    [Header("Настройки смерти")]
    [SerializeField] private float _deathAnimationDelay = 1.5f;

    // Зависимости через интерфейсы
    private IPlayerController _playerController;
    private IHealthUI _healthUI;
    private IDeathAnimator _deathAnimator;
    private ITimeManager _timeManager;

    private bool isDead = false;
    
    /// <summary>Событие вызывается при смерти игрока</summary>
    public event Action OnPlayerDied;

    // Реализация интерфейса IDamageable
    public bool IsDead => isDead;
    public Transform Transform => transform;

    /// <summary>Текущее здоровье</summary>
    public int CurrentHealth => currentHealth;

    /// <summary>Максимальное здоровье</summary>
    public int MaxHealth => maxHealth;

    void Start()
    {
        currentHealth = maxHealth;

        InitializeDependencies();

        var playerHealthUI = _healthUI as PlayerHealthUI;
        if (playerHealthUI != null)
            playerHealthUI.Initialize(maxHealth);

        Debug.Log($"[PlayerHealth] Инициализирован. Здоровье: {currentHealth}/{maxHealth}");
    }

    private void InitializeDependencies()
    {
        _playerController = GetComponent<IPlayerController>();
        _deathAnimator = GetComponent<IDeathAnimator>();
        _healthUI = FindObjectOfType<PlayerHealthUI>();

        // TimeManager получаем через инъекцию из SceneEntryPoint
        if (_timeManager == null)
            _timeManager = TimeManager.Instance;
    }

    /// <summary>
    /// Метод для инъекции TimeManager из SceneEntryPoint
    /// </summary>
    public void SetTimeManager(ITimeManager timeManager)
    {
        _timeManager = timeManager;
        Debug.Log("[PlayerHealth] TimeManager внедрен");
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            Debug.Log("[PlayerHealth] Игрок уже мертв, урон не применен");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"[PlayerHealth] Получен урон {damage}. Здоровье: {currentHealth}/{maxHealth}");

        if (_healthUI != null)
            _healthUI.UpdateHealth(currentHealth, maxHealth);

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

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[PlayerHealth] Игрок умер!");

        if (_playerController != null)
            _playerController.DisableControl();

        if (_deathAnimator != null)
            _deathAnimator.PlayDeathAnimation();

        if (_healthUI != null)
            _healthUI.HideHealthBar();

        // Вызываем событие смерти
        OnPlayerDied?.Invoke();

        Invoke(nameof(ShowDefeatMenu), _deathAnimationDelay);
    }

    private void ShowDefeatMenu()
    {
        Debug.Log("[PlayerHealth] Показ меню поражения");

        if (_timeManager != null)
            _timeManager.StopTime();
    }
}