using UnityEngine;

public class DefeatMenu : MonoBehaviour
{
    private UIManager _uiManager;

    /// <summary>
    /// Установка UIManager (вызывается из SceneEntryPoint)
    /// </summary>
    public void SetUIManager(UIManager uiManager)
    {
        _uiManager = uiManager;
        Debug.Log("[DefeatMenu] UIManager установлен");
    }

    /// <summary>
    /// Перезапустить уровень (вызывается по кнопке)
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log("[DefeatMenu] Нажата кнопка Restart");
        _uiManager?.RestartGame();
    }

    /// <summary>
    /// Выйти в главное меню (вызывается по кнопке)
    /// </summary>
    public void ExitToMenu()
    {
        Debug.Log("[DefeatMenu] Нажата кнопка Exit to Menu");
        _uiManager?.ExitToMenu();
    }
}