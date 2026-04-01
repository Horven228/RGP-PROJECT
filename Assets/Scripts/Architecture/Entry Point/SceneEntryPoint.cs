using UnityEngine;

/// <summary>
/// Локальная точка входа (Entry Point) для игровой сцены.
/// Настраивает объекты сцены и прокидывает зависимости.
/// </summary>
public class SceneEntryPoint : MonoBehaviour
{
    [Header("Player References")]
    [Tooltip("Компонент PlayerController на игроке")]
    [SerializeField] private PlayerController _playerController;

    [Tooltip("Компонент PlayerHealth на игроке")]
    [SerializeField] private PlayerHealth _playerHealth;

    [Tooltip("Компонент PlayerHealthUI (UI полоски здоровья)")]
    [SerializeField] private PlayerHealthUI _playerHealthUI;

    [Tooltip("Компонент PlayerDeathAnimator на игроке")]
    [SerializeField] private PlayerDeathAnimator _playerDeathAnimator;

    [Header("UI References")]
    [Tooltip("Объект с меню паузы (должен иметь компонент PauseMenu)")]
    [SerializeField] private GameObject _pauseMenu;

    [Tooltip("Объект с меню поражения (должен иметь компонент DefeatMenu)")]
    [SerializeField] private GameObject _defeatMenu;

    [Header("Audio")]
    [Tooltip("Объект со слайдером громкости (компонент Volume)")]
    [SerializeField] private Volume _volumeControl;

    private ITimeManager _timeManager;
    private IAudioService _audioService;
    private UIManager _uiManager;

    private void Awake()
    {
        // Получаем глобальные сервисы
        InitializeGlobalServices();

        // Прокидываем зависимости в компоненты
        InjectDependencies();

        // Создаем UI Manager
        CreateUIManager();

        // Настраиваем события
        SetupEvents();

        Debug.Log("[SceneEntryPoint] Сцена инициализирована");
    }

    private void InitializeGlobalServices()
    {
        _timeManager = ServiceLocator.Get<ITimeManager>();
        _audioService = ServiceLocator.Get<IAudioService>();

        if (_timeManager == null)
            Debug.LogError("[SceneEntryPoint] ITimeManager не найден в ServiceLocator!");
        if (_audioService == null)
            Debug.LogError("[SceneEntryPoint] IAudioService не найден в ServiceLocator!");
    }

    private void InjectDependencies()
    {
        // Прокидываем TimeManager в компоненты
        if (_playerHealth != null && _timeManager != null)
        {
            _playerHealth.SetTimeManager(_timeManager);

        }

        // Прокидываем AudioService в Volume
        if (_volumeControl != null && _audioService != null)
        {
            _volumeControl.SetAudioService(_audioService);

        }
    }

    private void CreateUIManager()
    {
        _uiManager = new UIManager(_pauseMenu, _defeatMenu, _timeManager, _playerController);

        // Передаем UIManager в меню
        if (_pauseMenu != null)
        {
            var pauseMenuComponent = _pauseMenu.GetComponent<PauseMenu>();
            if (pauseMenuComponent != null)
            {
                pauseMenuComponent.SetUIManager(_uiManager);

            }
            else
            {
                Debug.LogWarning("[SceneEntryPoint] На объекте PauseMenu нет компонента PauseMenu!");
            }
        }

        if (_defeatMenu != null)
        {
            var defeatMenuComponent = _defeatMenu.GetComponent<DefeatMenu>();
            if (defeatMenuComponent != null)
            {
                defeatMenuComponent.SetUIManager(_uiManager);
            }
            else
            {
                Debug.LogWarning("[SceneEntryPoint] На объекте DefeatMenu нет компонента DefeatMenu!");
            }
        }
    }

    private void SetupEvents()
    {
        // Подписываемся на смерть игрока
        if (_playerHealth != null)
        {
            _playerHealth.OnPlayerDied += HandlePlayerDeath;
        }
    }

    private void HandlePlayerDeath()
    {

        _uiManager?.ShowDefeatMenu();
    }

    private void Update()
    {
        // Обработка нажатия Escape для паузы
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _uiManager?.TogglePause();
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий
        if (_playerHealth != null)
        {
            _playerHealth.OnPlayerDied -= HandlePlayerDeath;
        }

        _uiManager?.Dispose();
        Debug.Log("[SceneEntryPoint] Сцена уничтожена, ресурсы очищены");
    }
}