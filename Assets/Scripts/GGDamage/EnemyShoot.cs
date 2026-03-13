using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private Animator _enemyAnimator;
    [SerializeField] private string _attackBoolName = "IsAttacking";
    [SerializeField] private string _deathBoolName = "Death";

    [Header("Кулдаун")]
    [SerializeField] private float _shootCooldown = 1f; // Кулдаун 1 секунда

    private float _lastShootTime = -1f; // Время последнего выстрела

    private void Update()
    {
        if (_enemyAnimator == null) return;

        // Проверяем, не мертв ли враг
        bool isDead = _enemyAnimator.GetBool(_deathBoolName);
        if (isDead) return;

        bool isAttacking = _enemyAnimator.GetBool(_attackBoolName);

        // Проверяем, прошел ли кулдаун
        if (isAttacking && Time.time >= _lastShootTime + _shootCooldown)
        {
            if (_projectilePrefab != null && _shootPoint != null)
            {
                Instantiate(_projectilePrefab, _shootPoint.position, _shootPoint.rotation);
                _lastShootTime = Time.time;
                Debug.Log("Снаряд выпущен!");
            }
        }
    }
}