using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Здоровье")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Эффекты")]
    [SerializeField] private GameObject _damageEffectPrefab;
    [SerializeField] private float _deathAnimationDelay = 1.5f;

    private IPlayerController _playerController;
    private IHealthUI _healthUI;
    private IDeathAnimator _deathAnimator;
    private ITimeManager _timeManager;

    private bool isDead = false;
    public event Action OnPlayerDied;

    public bool IsDead => isDead;
    public Transform Transform => transform;
    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        InitializeDependencies();
        _healthUI?.Initialize(maxHealth);
    }

    // МЕТОД ВОССТАНОВЛЕНИЯ
    public void RestoreHealth(int health)
    {
        isDead = false;
        currentHealth = health;
        if (_healthUI != null)
        {
            _healthUI.Initialize(maxHealth);
            _healthUI.UpdateHealth(currentHealth, maxHealth);
        }
        _playerController?.EnableControl();
    }

    private void InitializeDependencies()
    {
        _playerController = GetComponent<IPlayerController>();
        _deathAnimator = GetComponent<IDeathAnimator>();
        _healthUI = FindObjectOfType<PlayerHealthUI>();
        if (_timeManager == null) _timeManager = TimeManager.Instance;
    }

    public void SetTimeManager(ITimeManager timeManager) => _timeManager = timeManager;

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        _healthUI?.UpdateHealth(currentHealth, maxHealth);
        if (_damageEffectPrefab != null) Destroy(Instantiate(_damageEffectPrefab, transform.position, Quaternion.identity), 2f);
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        _playerController?.DisableControl();
        _deathAnimator?.PlayDeathAnimation();
        _healthUI?.HideHealthBar();
        OnPlayerDied?.Invoke();
        Invoke(nameof(ShowDefeatMenu), _deathAnimationDelay);
    }

    private void ShowDefeatMenu() => _timeManager?.StopTime();
}