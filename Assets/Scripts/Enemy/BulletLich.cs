using UnityEngine;

// Для маагического снаряда,  летит и наносит урон игроку
public class BulletLich : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _maxLifetime = 5f;

    [Header("Настройки урона")]
    [SerializeField] private int _damage = 20;
    [SerializeField] private bool _useTagFilter = true;
    [SerializeField] private string _targetTag = "Player";

    [Header("Эффекты")]
    [SerializeField] private GameObject _hitEffectPrefab;

    private float _spawnTime;

    private void Start()
    {
        _spawnTime = Time.time;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);

        if (Time.time - _spawnTime > _maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_useTagFilter && !other.CompareTag(_targetTag)) return;

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null && !damageable.IsDead)
        {
            damageable.TakeDamage(_damage);

            if (_hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            Destroy(gameObject);
        }
    }
}