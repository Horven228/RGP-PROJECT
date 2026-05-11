using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public static System.Action<GameObject> OnEnemyDeath;

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

    private IHealthUI _healthUI;
    private IDeathAnimator _deathAnimator;
    private NavMeshAgent _agent;
    private BaseEnemyStateMachine _stateMachine;

    private float lastDamageTime;
    private bool isDead = false;

    public System.Action<int, GameObject> OnTakeDamage;

    public bool IsDead => isDead;
    public Transform Transform => transform;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Start()
    {
        currentHealth = maxHealth;
        _agent = GetComponent<NavMeshAgent>();
        _stateMachine = GetComponent<BaseEnemyStateMachine>();
        InitializeDependencies();
        _healthUI?.Initialize(maxHealth);
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        _healthUI?.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    public void SetDeadFlag(bool dead)
    {
        isDead = dead;
    }

    private void InitializeDependencies()
    {
        _healthUI = GetComponent<IHealthUI>() ?? GetComponentInChildren<IHealthUI>();
        _deathAnimator = GetComponent<IDeathAnimator>() ?? GetComponentInChildren<IDeathAnimator>();
        if (_hitCollider == null) _hitCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead || Time.time - lastDamageTime < damageCooldown) return;
        if (!other.CompareTag(weaponTag)) return;

        Animator playerAnimator = other.GetComponentInParent<Animator>();
        if (playerAnimator != null && playerAnimator.GetBool("IsAttacking"))
        {
            TakeDamage(damageAmount);
            lastDamageTime = Time.time;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (_stateMachine != null)
        {
            _stateMachine.TriggerAggro(damage, gameObject);
        }

        OnTakeDamage?.Invoke(damage, gameObject);
        _healthUI?.UpdateHealth(currentHealth, maxHealth);

        if (_damageEffectPrefab != null)
            Destroy(Instantiate(_damageEffectPrefab, transform.position, Quaternion.identity), 2f);

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (_hitCollider != null) _hitCollider.enabled = false;
        if (_healthUI != null) _healthUI.HideHealthBar();
        if (_agent != null) _agent.enabled = false;

        if (_deathAnimator != null)
            _deathAnimator.PlayDeathAnimation();

        OnEnemyDeath?.Invoke(gameObject);

        Invoke(nameof(DisableObject), _destroyDelay);
    }

    private void DisableObject()
    {
        gameObject.SetActive(false);
    }
}