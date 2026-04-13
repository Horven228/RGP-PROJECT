using UnityEngine;
using System.Collections.Generic;

// ПАТТЕРН РЕПОЗИТОРИЙ (Отвечает только за запись/чтение файлов)
public interface ISaveRepository
{
    void Save(GameSaveData data);
    GameSaveData Load();
}

public class JsonSaveRepository : ISaveRepository
{
    private const string Key = "GameSaveSlot";

    public void Save(GameSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(Key, json);
        PlayerPrefs.Save();
        Debug.Log("Игра сохранена в JSON");
    }

    public GameSaveData Load()
    {
        if (!PlayerPrefs.HasKey(Key)) return null;
        string json = PlayerPrefs.GetString(Key);
        return JsonUtility.FromJson<GameSaveData>(json);
    }
}

// ПАТТЕРН ИНТЕРАКТОР (Связывает игру и репозиторий)
public class SaveLoadInteractor
{
    private readonly ISaveRepository _repository;

    public SaveLoadInteractor(ISaveRepository repository)
    {
        _repository = repository;
    }

    public void SaveGame(PlayerController player, PlayerHealth pHealth)
    {
        GameSaveData data = new GameSaveData();

        // Игрок - позиция
        data.player.position = new float[] {
            player.transform.position.x,
            player.transform.position.y,
            player.transform.position.z
        };

        // Игрок - здоровье
        data.player.health = pHealth.CurrentHealth;

        // Игрок - кулдаун магии
        data.player.magicCooldownRemaining = player.GetMagicCooldownRemaining();
        data.player.isMagicOnCooldown = player.IsMagicOnCooldown();

        // Враги
        EnemyHealth[] enemies = Object.FindObjectsOfType<EnemyHealth>(true);
        for (int i = 0; i < enemies.Length; i++)
        {
            // Создаем стабильный ID на основе имени и порядка в сцене
            string stableId = enemies[i].gameObject.name + "_" + i;

            data.enemies.Add(new EnemySaveData
            {
                id = stableId,
                position = new float[] {
                    enemies[i].transform.position.x,
                    enemies[i].transform.position.y,
                    enemies[i].transform.position.z
                },
                health = enemies[i].CurrentHealth,
                isDead = enemies[i].IsDead
            });
        }

        _repository.Save(data);

    }

    public void LoadGame(PlayerController player, PlayerHealth pHealth)
    {
        GameSaveData data = _repository.Load();
        if (data == null)
        {
            Debug.Log("Нет сохраненной игры");
            return;
        }

        Time.timeScale = 1f;

        // Загрузка игрока
        player.Teleport(new Vector3(
            data.player.position[0],
            data.player.position[1],
            data.player.position[2]
        ));
        pHealth.RestoreHealth(data.player.health);

        // Загрузка кулдауна магии
        player.RestoreMagicCooldown(data.player.magicCooldownRemaining, data.player.isMagicOnCooldown);

        // Загрузка врагов
        EnemyHealth[] sceneEnemies = Object.FindObjectsOfType<EnemyHealth>(true);
        foreach (var eData in data.enemies)
        {
            for (int i = 0; i < sceneEnemies.Length; i++)
            {
                string currentId = sceneEnemies[i].gameObject.name + "_" + i;
                if (currentId == eData.id)
                {
                    sceneEnemies[i].RestoreState(
                        eData.health,
                        new Vector3(eData.position[0], eData.position[1], eData.position[2]),
                        eData.isDead
                    );
                    break;
                }
            }
        }


    }
}
