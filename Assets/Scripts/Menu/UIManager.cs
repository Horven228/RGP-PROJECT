using UnityEngine;

/// <summary>
/// Управляет UI элементами сцены (меню паузы, поражения, победы)
/// </summary>
public class UIManager
{
    private readonly GameObject _pauseMenu;
    private readonly GameObject _defeatMenu;
    private readonly GameObject _winMenu;  // ДОБАВЬТЕ ЭТУ СТРОКУ
    private readonly ITimeManager _timeManager;
    private readonly IPlayerController _playerController;

    private bool _isPaused;

    // ОБНОВИТЕ КОНСТРУКТОР - добавьте winMenu
    public UIManager(GameObject pauseMenu, GameObject defeatMenu, ITimeManager timeManager, IPlayerController playerController, GameObject winMenu = null)
    {
        _pauseMenu = pauseMenu;
        _defeatMenu = defeatMenu;
        _winMenu = winMenu;  // ДОБАВЬТЕ
        _timeManager = timeManager;
        _playerController = playerController;

        // Инициализация меню - все выключено
        if (_pauseMenu != null)
            _pauseMenu.SetActive(false);
        if (_defeatMenu != null)
            _defeatMenu.SetActive(false);
        if (_winMenu != null)  // ДОБАВЬТЕ
            _winMenu.SetActive(false);

        // Настройка курсора для игры
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("[UIManager] Инициализирован");
    }

    // ДОБАВЬТЕ ЭТОТ МЕТОД
    public void ShowWinMenu()
    {
        Debug.Log("[UIManager] Показ меню победы");
        if (_winMenu != null)
            _winMenu.SetActive(true);

        _playerController?.DisableControl();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Остальные методы без изменений
    public void TogglePause()
    {
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

    public void ShowDefeatMenu()
    {
        Debug.Log("[UIManager] Показ меню поражения");
        _defeatMenu?.SetActive(true);
        _playerController?.DisableControl();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Debug.Log("[UIManager] Перезапуск игры");

        Time.timeScale = 1f;
        _timeManager?.ResumeTime();

        // Сбрасываем очки
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }

        // Перезагружаем сцену
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitToMenu()
    {
        Debug.Log("[UIManager] Выход в главное меню");
        _timeManager?.ResumeTime();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public void Dispose()
    {
        if (_defeatMenu != null && !_defeatMenu.activeSelf && _timeManager != null)
        {
            _timeManager.ResumeTime();
        }

        Debug.Log("[UIManager] Ресурсы очищены");
    }
}