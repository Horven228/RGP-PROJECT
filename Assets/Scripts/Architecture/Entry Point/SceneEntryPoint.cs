using UnityEngine;

public class SceneEntryPoint : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private PlayerHealth _playerHealth;

    [Header("UI References")]
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _defeatMenu;
    [SerializeField] private GameObject _winMenu;  // ДОБАВЬТЕ ЭТУ СТРОКУ
    [SerializeField] private SaveLoadView _saveLoadView;

    private ITimeManager _timeManager;
    private UIManager _uiManager;
    private SaveLoadController _saveLoadController;

    private void Awake()
    {
        InitializeGlobalServices();
        InjectDependencies();
        CreateUIManager();
        SetupEvents();
        InitSaveSystem();
    }

    private void InitializeGlobalServices()
    {
        _timeManager = ServiceLocator.Get<ITimeManager>();
    }

    private void InjectDependencies()
    {
        if (_playerHealth != null && _timeManager != null)
            _playerHealth.SetTimeManager(_timeManager);
    }

    private void CreateUIManager()
    {
        _uiManager = new UIManager(_pauseMenu, _defeatMenu, _timeManager, _playerController);

        if (_pauseMenu != null)
            _pauseMenu.GetComponent<PauseMenu>()?.SetUIManager(_uiManager);

        if (_defeatMenu != null)
            _defeatMenu.GetComponent<DefeatMenu>()?.SetUIManager(_uiManager);


        if (_winMenu != null)
        {
            Win winScript = _winMenu.GetComponent<Win>();
            if (winScript != null)
                winScript.SetUIManager(_uiManager);
        }
    }

    private void InitSaveSystem()
    {
        ISaveRepository repository = new JsonSaveRepository();
        SaveLoadInteractor interactor = new SaveLoadInteractor(repository);
        _saveLoadController = new SaveLoadController(interactor, _playerController, _playerHealth);

        if (_saveLoadView != null)
            _saveLoadView.Initialize(_saveLoadController);
    }

    private void SetupEvents()
    {
        if (_playerHealth != null)
            _playerHealth.OnPlayerDied += () => _uiManager?.ShowDefeatMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            _uiManager?.TogglePause();
    }
}