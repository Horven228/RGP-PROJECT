using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Здоровье")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Настройки урона")]
    [SerializeField] private string weaponTag = "Weapon";
    [SerializeField] private int damageAmount = 20;
    [SerializeField] private float damageCooldown = 0.5f;

    [Header("Эффекты")]
    [SerializeField] private GameObject _damageEffectPrefab;

    [Header("Настройки смерти")]
    [SerializeField] private float _destroyDelay = 2f;
    [SerializeField] private Collider _hitCollider;

    // Зависимости через интерфейсы
    private IHealthUI _healthUI;
    private IDeathAnimator _deathAnimator;

    private float lastDamageTime;
    private bool isDead = false;

    // Реализация интерфейса IDamageable
    public bool IsDead => isDead;
    public Transform Transform => transform;

    void Start()
    {
        currentHealth = maxHealth;

        // Инициализируем зависимости
        InitializeDependencies();

        // Инициализируем UI отдельно
        var enemyHealthUI = _healthUI as EnemyHealthUI;
        if (enemyHealthUI != null)
            enemyHealthUI.Initialize(maxHealth);
    }

    private void InitializeDependencies()
    {
        // Ищем UI на враге
        _healthUI = GetComponent<IHealthUI>();
        if (_healthUI == null)
            _healthUI = GetComponentInChildren<IHealthUI>();

        // Ищем аниматор смерти
        _deathAnimator = GetComponent<IDeathAnimator>();
        if (_deathAnimator == null)
            _deathAnimator = GetComponentInChildren<IDeathAnimator>();

        if (_hitCollider == null)
            _hitCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (Time.time - lastDamageTime < damageCooldown) return;
        if (!other.CompareTag(weaponTag)) return;

        Animator playerAnimator = other.GetComponentInParent<Animator>();
        if (playerAnimator == null) return;

        bool isAttacking = playerAnimator.GetBool("IsAttacking");
        if (!isAttacking) return;

        TakeDamage(damageAmount);
        lastDamageTime = Time.time;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

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

        if (_hitCollider != null)
            _hitCollider.enabled = false;

        if (_healthUI != null)
            _healthUI.HideHealthBar();

        if (_deathAnimator != null)
            _deathAnimator.PlayDeathAnimation();

        Destroy(gameObject, _destroyDelay);
    }
}