using UnityEngine;

//  создает магиченские снаряды для врага-мага
public class EnemyShoot : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private Animator _enemyAnimator;
    [SerializeField] private string _attackBoolName = "IsAttacking";
    [SerializeField] private string _deathBoolName = "Death";

    [Header("Кулдаун")]
    [SerializeField] private float _shootCooldown = 1f; // Кулдаун между выстрелами
    [SerializeField] private float _initialShootDelay = 1f; // Задержка перед первым выстрелом

    private float _lastShootTime = -1f; // Время последнего выстрела
    private float _enterAttackTime = -1f; // Время входа в состояние атаки
    private bool _wasAttacking = false; // Флаг для отслеживания входа в атаку

    private void Update()
    {
        if (_enemyAnimator == null) return;

        // Проверяем, не мертв ли враг
        bool isDead = _enemyAnimator.GetBool(_deathBoolName);
        if (isDead) return;

        bool isAttacking = _enemyAnimator.GetBool(_attackBoolName);

        // Отслеживаем момент входа в состояние атаки
        if (isAttacking && !_wasAttacking)
        {
            _enterAttackTime = Time.time;
            _wasAttacking = true;
        }

        // Отслеживаем выход из состояния атаки
        if (!isAttacking && _wasAttacking)
        {
            _wasAttacking = false;
        }

        // Проверяем, можно ли стрелять
        if (isAttacking && _wasAttacking)
        {
            // Проверяем, прошла ли начальная задержка
            if (Time.time >= _enterAttackTime + _initialShootDelay)
            {
                // Проверяем, прошел ли кулдаун между выстрелами
                if (Time.time >= _lastShootTime + _shootCooldown)
                {
                    if (_projectilePrefab != null && _shootPoint != null)
                    {
                        Instantiate(_projectilePrefab, _shootPoint.position, _shootPoint.rotation);
                        _lastShootTime = Time.time;
                    }
                }
            }
        }
    }
}