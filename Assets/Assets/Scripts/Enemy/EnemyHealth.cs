using UnityEngine;
using UnityEngine.AI;

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
    [SerializeField] private float _destroyDelay = 2f; // Время для анимации
    [SerializeField] private Collider _hitCollider;

    private IHealthUI _healthUI;
    private IDeathAnimator _deathAnimator;
    private NavMeshAgent _agent;

    private float lastDamageTime;
    private bool isDead = false;

    public bool IsDead => isDead;
    public Transform Transform => transform;
    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        _agent = GetComponent<NavMeshAgent>();
        InitializeDependencies();
        _healthUI?.Initialize(maxHealth);
    }

    public void RestoreState(int health, Vector3 pos, bool dead)
    {
        if (dead)
        {
            isDead = true;
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        isDead = false;
        currentHealth = health;

        // Если есть NavMeshAgent, перемещаем через Warp
        if (_agent != null)
        {
            _agent.enabled = true;
            _agent.Warp(pos);
        }
        else
        {
            transform.position = pos;
        }

        if (_hitCollider != null) _hitCollider.enabled = true;
        _healthUI?.Initialize(maxHealth);
        _healthUI?.UpdateHealth(currentHealth, maxHealth);

        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.SetBool("Death", false);
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
        _healthUI?.UpdateHealth(currentHealth, maxHealth);

        if (_damageEffectPrefab != null)
            Destroy(Instantiate(_damageEffectPrefab, transform.position, Quaternion.identity), 2f);

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (_hitCollider != null) _hitCollider.enabled = false;
        if (_healthUI != null) _healthUI.HideHealthBar();
        if (_agent != null) _agent.enabled = false;

        if (_deathAnimator != null)
            _deathAnimator.PlayDeathAnimation();

        // Отключаем объект только ПОСЛЕ анимации
        Invoke(nameof(DisableObject), _destroyDelay);
    }

    private void DisableObject() => gameObject.SetActive(false);
}