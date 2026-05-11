using UnityEngine;
using System.Collections.Generic;

public class BossSpawnerByScore : MonoBehaviour
{
    [Header("Настройки босса")]
    [SerializeField] private GameObject _bossPrefab;
    [SerializeField] private Transform _bossSpawnPoint;
    [SerializeField] private int _requiredScore = 4;

    [Header("Точки патруля для босса")]
    [SerializeField] private List<Transform> _patrolPoints;

    private bool _bossSpawned = false;
    private bool _isLoading = false;
    private int _lastCheckedScore = 0;  // ← ДОБАВИТЬ: для отслеживания изменения очков

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            _lastCheckedScore = ScoreManager.Instance.GetScore();  // ← запоминаем текущие очки
            CheckScore(_lastCheckedScore);
            ScoreManager.Instance.OnScoreChanged += CheckScore;
        }
        else
        {
            Debug.LogError("[BossSpawner] ScoreManager не найден!");
        }
    }

    public void SetLoading(bool loading)
    {
        _isLoading = loading;

        // Если загрузка завершена, обновляем последние проверенные очки
        if (!loading && ScoreManager.Instance != null)
        {
            _lastCheckedScore = ScoreManager.Instance.GetScore();
            Debug.Log($"[BossSpawner] Загрузка завершена, текущие очки: {_lastCheckedScore}, требуется: {_requiredScore}");
        }
    }

    public void SetBossSpawned(bool spawned)
    {
        _bossSpawned = spawned;
        Debug.Log($"[BossSpawner] SetBossSpawned = {spawned}");
    }

    public bool IsBossSpawned()
    {
        return _bossSpawned;
    }

    public void ForceSpawnBoss()
    {
        if (!_bossSpawned)
        {
            SpawnBoss();
        }
    }

    private void CheckScore(int currentScore)
    {
        Debug.Log($"[BossSpawner] CheckScore: currentScore={currentScore}, required={_requiredScore}, _isLoading={_isLoading}, _bossSpawned={_bossSpawned}, _lastCheckedScore={_lastCheckedScore}");

        if (_isLoading)
        {
            Debug.Log("[BossSpawner] Пропускаем проверку - идет загрузка");
            return;
        }

        if (_bossSpawned)
        {
            Debug.Log("[BossSpawner] Босс уже заспавнен");
            return;
        }

        // ✅ НОВАЯ ПРОВЕРКА: если очки достигнуты ИЛИ превышены
        if (currentScore >= _requiredScore)
        {
            Debug.Log($"[BossSpawner] Условие выполнено! currentScore={currentScore} >= {_requiredScore}");
            SpawnBoss();
        }
        else
        {
            Debug.Log($"[BossSpawner] Условие НЕ выполнено: {currentScore} < {_requiredScore}");
        }

        _lastCheckedScore = currentScore;
    }

    private void SpawnBoss()
    {
        if (_bossPrefab == null)
        {
            Debug.LogError("[BossSpawner] Префаб босса не назначен!");
            return;
        }

        // ✅ ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА: если босс уже существует в сцене
        GameObject existingBoss = GameObject.Find("WizardBoss");
        if (existingBoss != null)
        {
            Debug.Log("[BossSpawner] Босс уже существует в сцене, активируем его");
            existingBoss.SetActive(true);
            _bossSpawned = true;
            return;
        }

        _bossSpawned = true;

        Vector3 spawnPos = _bossSpawnPoint != null ? _bossSpawnPoint.position : transform.position;
        GameObject boss = Instantiate(_bossPrefab, spawnPos, Quaternion.identity);
        boss.name = "WizardBoss";

        AssignPatrolPoints(boss);

        Debug.Log($"[BossSpawner] Босс появился! Очки: {_requiredScore}");
    }

    private void AssignPatrolPoints(GameObject boss)
    {
        BaseEnemyStateMachine stateMachine = boss.GetComponent<BaseEnemyStateMachine>();
        if (stateMachine != null && _patrolPoints.Count > 0)
        {
            stateMachine.PatrolPoints = new List<Transform>(_patrolPoints);
        }
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= CheckScore;
        }
    }
}