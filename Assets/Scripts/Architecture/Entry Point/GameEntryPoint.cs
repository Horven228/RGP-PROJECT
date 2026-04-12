using UnityEngine;

/// <summary>
/// Глобальная точка входа (Entry Point) для всего приложения.
/// Создается один раз при запуске игры.
/// Только регистрирует сервисы, НЕ загружает сцены.
/// </summary>
public class GameEntryPoint : MonoBehaviour
{
    private void Awake()
    {
        // Регистрируем все глобальные сервисы
        RegisterServices();

        // Не уничтожаем при загрузке новых сцен
        DontDestroyOnLoad(gameObject);

        Debug.Log("[GameEntryPoint] Инициализация завершена");
    }

    private void RegisterServices()
    {
        // Создаем и регистрируем AudioService
        var audioService = new AudioService();
        ServiceLocator.Register<IAudioService>(audioService);

        Debug.Log("[GameEntryPoint] Сервисы зарегистрированы: IAudioService");
    }
}