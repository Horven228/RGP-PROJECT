using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    [SerializeField] private List<GameObject> _enemyPrefabs;
    [SerializeField] private int _maxEnemies = 3;
    [SerializeField] private float _spawnRadius = 10f;
    [SerializeField] private float _spawnDelay = 5f;
    [SerializeField] private bool _spawnOnStart = true;

    [Header("Точки патруля (перетащите сюда)")]
    [SerializeField] private List<Transform> _patrolPoints;

    private float _nextSpawnTime;
    private List<GameObject> _activeEnemies = new List<GameObject>();

    private void Start()
    {
        if (_spawnOnStart)
        {
            _nextSpawnTime = Time.time;
        }
    }

    private void Update()
    {
        _activeEnemies.RemoveAll(enemy => enemy == null);

        if (_activeEnemies.Count >= _maxEnemies) return;
        if (Time.time < _nextSpawnTime) return;

        SpawnEnemy();
        _nextSpawnTime = Time.time + _spawnDelay;
    }

    private void SpawnEnemy()
    {
        if (_enemyPrefabs.Count == 0) return;

        // Выбираем случайного врага
        GameObject enemyPrefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Count)];

        // Случайная позиция в радиусе
        Vector2 randomCircle = Random.insideUnitCircle * _spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        // Создаем врага
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        _activeEnemies.Add(newEnemy);

        // Назначаем точки патруля
        AssignPatrolPoints(newEnemy);
    }

    private void AssignPatrolPoints(GameObject enemy)
    {
        BaseEnemyStateMachine stateMachine = enemy.GetComponent<BaseEnemyStateMachine>();
        if (stateMachine != null && _patrolPoints.Count > 0)
        {
            stateMachine.PatrolPoints = new List<Transform>(_patrolPoints);
            Debug.Log($"[EnemySpawner] {enemy.name} получил {_patrolPoints.Count} точек патруля");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _spawnRadius);
    }
}
