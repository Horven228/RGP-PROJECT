using UnityEngine;

public class GameModeManager : MonoBehaviour
{
    private static GameModeManager _instance;
    public static GameModeManager Instance => _instance;

    [SerializeField] private GameMode _currentMode = GameMode.Normal;

    public GameMode CurrentMode
    {
        get => _currentMode;
        set
        {
            _currentMode = value;
            OnGameModeChanged?.Invoke(_currentMode);
            Debug.Log($"Режим игры изменён на: {_currentMode}");
        }
    }

    public event System.Action<GameMode> OnGameModeChanged;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}