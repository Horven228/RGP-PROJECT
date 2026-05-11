using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _maxLifetime = 5f;

    [Header("Настройки урона")]
    [SerializeField] private int _damage = 20;
    [SerializeField] private string _targetTag = "Player";

    [Header("Эффекты")]
    [SerializeField] private GameObject _hitEffectPrefab;

    private float _spawnTime;
    private GameObject _hitEffect;  // Эффект, переданный из босса

    private void Start()
    {
        _spawnTime = Time.time;
    }

    /// <summary>
    /// Инициализация снаряда с эффектом от оружия
    /// </summary>
    public void Initialize(GameObject hitEffect)
    {
        _hitEffect = hitEffect;
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
        if (!other.CompareTag(_targetTag)) return;

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable != null && !damageable.IsDead)
        {
            damageable.TakeDamage(_damage);
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }

    private void SpawnHitEffect()
    {
        GameObject effectToSpawn = _hitEffect != null ? _hitEffect : _hitEffectPrefab;

        if (effectToSpawn != null)
        {
            GameObject effect = Instantiate(effectToSpawn, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
}
