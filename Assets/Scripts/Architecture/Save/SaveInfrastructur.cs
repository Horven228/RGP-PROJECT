using UnityEngine;
using System.Collections.Generic;

// ===========================================
// 1. МОДЕЛИ ДАННЫХ (Data Models)
// ===========================================

[System.Serializable]
public class PlayerSaveData
{
    public float[] position;
    public int health;
    public float magicCooldownRemaining;
    public bool isMagicOnCooldown;
    public int score;
}

[System.Serializable]
public class EnemySaveData
{
    public string id;
    public float[] position;
    public int health;
    public bool isDead;
    public bool isAgro;
}

[System.Serializable]
public class BossSaveData
{
    public float[] position;
    public int health;
    public bool isDead;
    public bool isSpawned;
    public bool isAgro;
    public float agroRemainingTime;
    public bool isPhase2;
}

[System.Serializable]
public class GameSaveData
{
    public PlayerSaveData player = new PlayerSaveData();
    public List<EnemySaveData> enemies = new List<EnemySaveData>();
    public BossSaveData boss = new BossSaveData();
}

// ===========================================
// 2. РЕПОЗИТОРИЙ
// ===========================================

public interface ISaveRepository
{
    void Save(GameSaveData data);
    GameSaveData Load();
}

public class JsonSaveRepository : ISaveRepository
{
    private const string SaveKey = "GameSaveSlot";

    public void Save(GameSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        Debug.Log("[Repository] Игра сохранена");
    }

    public GameSaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return null;

        string json = PlayerPrefs.GetString(SaveKey);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
        Debug.Log("[Repository] Игра загружена");
        return data;
    }
}

// ===========================================
// 3. ИНТЕРАКТОР
// ===========================================

public class SaveLoadInteractor
{
    private GameSaveData _data;
    private readonly ISaveRepository _repository;

    public SaveLoadInteractor(ISaveRepository repository)
    {
        _repository = repository;
        _data = new GameSaveData();
    }

    public void SaveGame(PlayerController player, PlayerHealth pHealth)
    {
        _data.player = new PlayerSaveData
        {
            position = new float[] {
                player.transform.position.x,
                player.transform.position.y,
                player.transform.position.z
            },
            health = pHealth.CurrentHealth,
            magicCooldownRemaining = player.GetMagicCooldownRemaining(),
            isMagicOnCooldown = player.IsMagicOnCooldown(),
            score = ScoreManager.Instance != null ? ScoreManager.Instance.GetScore() : 0
        };

        // Сохраняем обычных врагов
        _data.enemies.Clear();
        EnemyHealth[] enemies = Object.FindObjectsOfType<EnemyHealth>(true);

        foreach (var enemy in enemies)
        {
            if (enemy.gameObject.name == "WizardBoss") continue;

            string enemyId = enemy.gameObject.name + "_" + enemy.GetInstanceID();

            BaseEnemyStateMachine stateMachine = enemy.GetComponent<BaseEnemyStateMachine>();
            bool isAgro = stateMachine != null && stateMachine.IsAgro;

            _data.enemies.Add(new EnemySaveData
            {
                id = enemyId,
                position = new float[] {
                    enemy.transform.position.x,
                    enemy.transform.position.y,
                    enemy.transform.position.z
                },
                health = enemy.CurrentHealth,
                isDead = enemy.IsDead,
                isAgro = isAgro
            });
        }

        // Сохраняем босса
        SaveBossData();

        _repository.Save(_data);

        int aliveCount = 0;
        foreach (var e in _data.enemies)
            if (!e.isDead) aliveCount++;

        Debug.Log($"[SaveLoad] Сохранено! Игрок: HP={_data.player.health}, Врагов: {_data.enemies.Count} (живых: {aliveCount})");
    }

    private void SaveBossData()
    {
        GameObject boss = GameObject.Find("WizardBoss");

        if (boss != null && boss.activeInHierarchy)
        {
            EnemyHealth bossHealth = boss.GetComponent<EnemyHealth>();
            BaseEnemyStateMachine bossStateMachine = boss.GetComponent<BaseEnemyStateMachine>();
            BossWizardStateMachine bossWizard = boss.GetComponent<BossWizardStateMachine>();

            if (bossHealth != null)
            {
                float agroRemainingTime = 0f;
                if (bossStateMachine != null && bossStateMachine.IsAgro)
                {
                    agroRemainingTime = bossStateMachine.GetAgroRemainingTime();
                }

                bool isPhase2 = bossWizard != null && bossWizard.IsInPhase2();

                _data.boss = new BossSaveData
                {
                    position = new float[] {
                        boss.transform.position.x,
                        boss.transform.position.y,
                        boss.transform.position.z
                    },
                    health = bossHealth.CurrentHealth,
                    isDead = bossHealth.IsDead,
                    isSpawned = true,
                    isAgro = bossStateMachine != null && bossStateMachine.IsAgro,
                    agroRemainingTime = agroRemainingTime,
                    isPhase2 = isPhase2
                };
            }
        }
        else
        {
            _data.boss.isSpawned = false;
            _data.boss.isDead = false;
            _data.boss.isAgro = false;
            _data.boss.agroRemainingTime = 0f;
            _data.boss.isPhase2 = false;
        }
    }

    public void LoadGame(PlayerController player, PlayerHealth pHealth)
    {
        GameSaveData loadedData = _repository.Load();
        if (loadedData == null)
        {
            Debug.Log("[SaveLoad] Нет сохраненной игры");
            return;
        }

        _data = loadedData;
        Time.timeScale = 1f;

        // Восстанавливаем игрока
        player.Teleport(new Vector3(
            _data.player.position[0],
            _data.player.position[1],
            _data.player.position[2]
        ));
        pHealth.RestoreHealth(_data.player.health);
        player.RestoreMagicCooldown(_data.player.magicCooldownRemaining, _data.player.isMagicOnCooldown);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SetScore(_data.player.score);
        }

        // Восстанавливаем обычных врагов
        RestoreRegularEnemies();

        // Восстанавливаем босса
        RestoreBoss();

        Debug.Log($"[SaveLoad] Загрузка завершена!");
    }

    private void RestoreRegularEnemies()
    {
        EnemyHealth[] existingEnemies = Object.FindObjectsOfType<EnemyHealth>(true);
        HashSet<string> restoredEnemies = new HashSet<string>();

        foreach (var eData in _data.enemies)
        {
            EnemyHealth targetEnemy = FindEnemyById(existingEnemies, eData.id);

            if (targetEnemy != null)
            {
                EnemyStateRestorer restorer = targetEnemy.GetComponent<EnemyStateRestorer>();

                if (restorer != null)
                {
                    Vector3 pos = new Vector3(eData.position[0], eData.position[1], eData.position[2]);
                    restorer.RestoreState(eData.health, pos, eData.isDead, eData.isAgro, 0f);
                }
                else
                {
                    if (!eData.isDead)
                    {
                        targetEnemy.gameObject.SetActive(true);
                        targetEnemy.SetHealth(eData.health);
                        targetEnemy.transform.position = new Vector3(eData.position[0], eData.position[1], eData.position[2]);
                    }
                    else
                    {
                        targetEnemy.gameObject.SetActive(false);
                    }
                }

                restoredEnemies.Add(eData.id);
            }
        }

        foreach (var enemy in existingEnemies)
        {
            if (enemy.gameObject.name == "WizardBoss") continue;

            string enemyId = enemy.gameObject.name + "_" + enemy.GetInstanceID();
            if (!restoredEnemies.Contains(enemyId))
            {
                Object.Destroy(enemy.gameObject);
            }
        }
    }

    private void RestoreBoss()
    {
        BossSpawnerByScore bossSpawner = Object.FindObjectOfType<BossSpawnerByScore>();

        // Если босс НЕ должен быть заспавнен
        if (!_data.boss.isSpawned)
        {
            GameObject existingBoss = GameObject.Find("WizardBoss");
            if (existingBoss != null)
            {
                Object.Destroy(existingBoss);
            }

            if (bossSpawner != null)
            {
                bossSpawner.SetBossSpawned(false);
            }
            return;
        }

        // Если босс должен быть мертв
        if (_data.boss.isDead)
        {
            GameObject existingBoss = GameObject.Find("WizardBoss");
            if (existingBoss != null)
            {
                existingBoss.SetActive(false);
            }

            if (bossSpawner != null)
            {
                bossSpawner.SetBossSpawned(true);
            }
            return;
        }

        // Босс должен быть жив
        GameObject boss = GameObject.Find("WizardBoss");

        if (boss != null)
        {
            EnemyStateRestorer bossRestorer = boss.GetComponent<EnemyStateRestorer>();
            BossWizardStateMachine bossWizard = boss.GetComponent<BossWizardStateMachine>();

            if (bossRestorer != null)
            {
                Vector3 bossPos = new Vector3(_data.boss.position[0], _data.boss.position[1], _data.boss.position[2]);
                bossRestorer.RestoreState(
                    _data.boss.health,
                    bossPos,
                    false,
                    _data.boss.isAgro,
                    _data.boss.agroRemainingTime
                );
            }

            if (bossWizard != null)
            {
                bossWizard.SetPhase(_data.boss.isPhase2);
                Debug.Log($"[SaveLoad] Босс восстановлен с фазой: {(_data.boss.isPhase2 ? "Phase2" : "Phase1")}");
            }

            if (bossSpawner != null)
            {
                bossSpawner.SetBossSpawned(true);
            }
        }
        else
        {
            if (bossSpawner != null)
            {
                Debug.Log("[SaveLoad] Создаем босса через спавнер...");

                bossSpawner.SetLoading(true);
                bossSpawner.SetBossSpawned(false);
                bossSpawner.ForceSpawnBoss();
                bossSpawner.SetLoading(false);

                GameObject newBoss = GameObject.Find("WizardBoss");
                if (newBoss != null)
                {
                    EnemyStateRestorer bossRestorer = newBoss.GetComponent<EnemyStateRestorer>();
                    BossWizardStateMachine bossWizard = newBoss.GetComponent<BossWizardStateMachine>();

                    if (bossRestorer != null)
                    {
                        Vector3 bossPos = new Vector3(_data.boss.position[0], _data.boss.position[1], _data.boss.position[2]);
                        bossRestorer.RestoreState(
                            _data.boss.health,
                            bossPos,
                            false,
                            _data.boss.isAgro,
                            _data.boss.agroRemainingTime
                        );
                    }

                    if (bossWizard != null)
                    {
                        bossWizard.SetPhase(_data.boss.isPhase2);
                    }
                }
            }
            else
            {
                Debug.LogError("[SaveLoad] Не найден BossSpawnerByScore!");
            }
        }
    }

    private EnemyHealth FindEnemyById(EnemyHealth[] enemies, string id)
    {
        foreach (var enemy in enemies)
        {
            if (enemy.gameObject.name == "WizardBoss") continue;

            string enemyId = enemy.gameObject.name + "_" + enemy.GetInstanceID();
            if (enemyId == id)
            {
                return enemy;
            }
        }
        return null;
    }
}

// ===========================================
// 4. КОНТРОЛЛЕР
// ===========================================

public class SaveLoadController
{
    private readonly SaveLoadInteractor _interactor;
    private readonly PlayerController _player;
    private readonly PlayerHealth _pHealth;

    public SaveLoadController(SaveLoadInteractor interactor, PlayerController player, PlayerHealth pHealth)
    {
        _interactor = interactor;
        _player = player;
        _pHealth = pHealth;
    }

    public void OnSaveClicked()
    {
        Debug.Log("[Controller] Нажата кнопка Save");
        _interactor.SaveGame(_player, _pHealth);
    }

    public void OnLoadClicked()
    {
        Debug.Log("[Controller] Нажата кнопка Load");
        _interactor.LoadGame(_player, _pHealth);
    }
}