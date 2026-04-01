using UnityEngine;

/// <summary>
/// Управляет UI элементами сцены (меню паузы, поражения)
/// </summary>
public class UIManager
{
    private readonly GameObject _pauseMenu;
    private readonly GameObject _defeatMenu;
    private readonly ITimeManager _timeManager;
    private readonly IPlayerController _playerController;

    private bool _isPaused;

    public UIManager(GameObject pauseMenu, GameObject defeatMenu, ITimeManager timeManager, IPlayerController playerController)
    {
        _pauseMenu = pauseMenu;
        _defeatMenu = defeatMenu;
        _timeManager = timeManager;
        _playerController = playerController;

        // Инициализация меню - все выключено
        if (_pauseMenu != null)
            _pauseMenu.SetActive(false);
        if (_defeatMenu != null)
            _defeatMenu.SetActive(false);

        // Настройка курсора для игры
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[UIManager] Инициализирован");
    }

    /// <summary>
    /// Переключить состояние паузы
    /// </summary>
    public void TogglePause()
    {
        // Нельзя открыть паузу если уже показано меню поражения
        if (_defeatMenu != null && _defeatMenu.activeSelf)
        {
            Debug.Log("[UIManager] Нельзя открыть паузу, так как активно меню поражения");
            return;
        }

        _isPaused = !_isPaused;

        if (_isPaused)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    private void Pause()
    {
        Debug.Log("[UIManager] Пауза активирована");
        _pauseMenu?.SetActive(true);
        _timeManager?.StopTime();
        _playerController?.DisableControl();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Resume()
    {
        Debug.Log("[UIManager] Пауза снята");
        _pauseMenu?.SetActive(false);
        _timeManager?.ResumeTime();
        _playerController?.EnableControl();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Показать меню поражения
    /// </summary>
    public void ShowDefeatMenu()
    {
        Debug.Log("[UIManager] Показ меню поражения");
        _defeatMenu?.SetActive(true);
        _playerController?.DisableControl();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Перезапустить игру
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("[UIManager] Перезапуск игры");
        _timeManager?.ResumeTime();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Выйти в главное меню
    /// </summary>
    public void ExitToMenu()
    {
        Debug.Log("[UIManager] Выход в главное меню");
        _timeManager?.ResumeTime();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// Очистка ресурсов
    /// </summary>
    public void Dispose()
    {
        // Если меню поражения не активно, возобновляем время
        if (_defeatMenu != null && !_defeatMenu.activeSelf && _timeManager != null)
        {
            _timeManager.ResumeTime();
        }

        Debug.Log("[UIManager] Ресурсы очищены");
    }
}