using System.Collections;
using UnityEngine;

public class EnemyHandDamage : MonoBehaviour
{
    [Header("Настройки урона")]
    [SerializeField] private int damageAmount = 20;
    [SerializeField] private float damageCooldown = 1f;

    [Header("Компоненты")]
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private string attackBoolName = "IsAttacking";

    private float lastDamageTime;
    private bool canDealDamage = true;

    void Start()
    {
        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponentInParent<Animator>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDealDamage) return;
        if (Time.time - lastDamageTime < damageCooldown) return;

        if (enemyAnimator != null)
        {
            bool isAttacking = enemyAnimator.GetBool(attackBoolName);
            if (!isAttacking) return;
        }

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null && !damageable.IsDead)
        {
            damageable.TakeDamage(damageAmount);
            lastDamageTime = Time.time;
            Debug.Log($"{gameObject.name} ударил {damageable.Transform.name}!");

            StartCoroutine(DamageCooldownRoutine());
        }
    }

    private IEnumerator DamageCooldownRoutine()
    {
        canDealDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canDealDamage = true;
    }
}