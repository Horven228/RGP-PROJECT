using UnityEngine;

public class EnemyHandDamage : MonoBehaviour
{
    [Header("Настройки урона")]
    [SerializeField] private int damageAmount = 20;
    [SerializeField] private int strongDamageAmount = 40;

    [Header("Компоненты")]
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private string attackBoolName = "IsAttacking";
    [SerializeField] private string strongAttackBoolName = "IsStrongAttacking";

    private bool _hasHitInCurrentAttack = false; // Флаг: был ли уже удар в этой анимации

    void Start()
    {
        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponentInParent<Animator>();
        }
    }

    void Update()
    {
        // Проверяем, идет ли сейчас какая-либо атака
        bool isAnyAttacking = enemyAnimator.GetBool(attackBoolName) || enemyAnimator.GetBool(strongAttackBoolName);

        // Если враг НЕ атакует (вернулся в Idle или Chase), сбрасываем флаг для следующего удара
        if (!isAnyAttacking)
        {
            _hasHitInCurrentAttack = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Если мы уже нанесли урон в этом взмахе — выходим
        if (_hasHitInCurrentAttack) return;

        // 2. Проверяем, включена ли атака в аниматоре в данный момент
        bool isNormalAttack = enemyAnimator.GetBool(attackBoolName);
        bool isStrongAttack = enemyAnimator.GetBool(strongAttackBoolName);

        if (!isNormalAttack && !isStrongAttack) return;

        // 3. Пытаемся получить компонент здоровья
        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null && !damageable.IsDead)
        {
            // Определяем урон в зависимости от типа анимации
            int finalDamage = isStrongAttack ? strongDamageAmount : damageAmount;

            damageable.TakeDamage(finalDamage);

            // 4. ГЛАВНОЕ: ставим флаг, что урон в этой анимации уже нанесен
            _hasHitInCurrentAttack = true;

            Debug.Log($"Урон {finalDamage} нанесен. Следующий удар возможен только после новой анимации.");
        }
    }
}