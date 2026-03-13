using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Настройки урона")]
    [SerializeField] private string weaponTag = "Weapon";
    [SerializeField] private int damageAmount = 20;
    [SerializeField] private float damageCooldown = 0.5f;

    [Header("Эффекты")]
    [SerializeField] private GameObject _damageEffectPrefab;

    [Header("Шкала здоровья")]
    [SerializeField] private Slider _healthSlider;

    [Header("Настройки смерти")]
    [SerializeField] private Animator _enemyAnimator;
    [SerializeField] private string _deathBoolName = "Death";
    [SerializeField] private float _destroyDelay = 2f;
    [SerializeField] private Collider _hitCollider;

    private float lastDamageTime;
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

        if (_enemyAnimator == null)
        {
            _enemyAnimator = GetComponent<Animator>();
        }

        if (_hitCollider == null)
        {
            _hitCollider = GetComponent<Collider>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (Time.time - lastDamageTime < damageCooldown) return;
        if (!other.CompareTag(weaponTag)) return;

        Animator playerAnimator = other.GetComponentInParent<Animator>();

        if (playerAnimator == null)
        {
            Debug.LogWarning("Не найден Animator на игроке");
            return;
        }

        bool isAttacking = playerAnimator.GetBool("IsAttacking");

        if (!isAttacking) return;

        TakeDamage(damageAmount);
        lastDamageTime = Time.time;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (_healthSlider != null)
        {
            _healthSlider.value = currentHealth;
        }

        if (_damageEffectPrefab != null)
        {
            GameObject effect = Instantiate(_damageEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        Debug.Log($"Враг получил урон. Осталось здоровья: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Враг умер!");

        if (_hitCollider != null)
        {
            _hitCollider.enabled = false;
        }

        if (_healthSlider != null)
        {
            _healthSlider.gameObject.SetActive(false);
        }

        if (_enemyAnimator != null && !string.IsNullOrEmpty(_deathBoolName))
        {
            _enemyAnimator.SetBool(_deathBoolName, true);
        }

        Destroy(gameObject, _destroyDelay);
    }
}