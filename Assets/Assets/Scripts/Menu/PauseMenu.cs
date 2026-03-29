using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private UIManager _uiManager;

    /// <summary>
    /// Установка UIManager (вызывается из SceneEntryPoint)
    /// </summary>
    public void SetUIManager(UIManager uiManager)
    {
        _uiManager = uiManager;
        Debug.Log("[PauseMenu] UIManager установлен");
    }

    public void ExitToMainMenu()
    {
        Debug.Log("[PauseMenu] Выход в главное меню");
        Time.timeScale = 1f;
        _uiManager?.ExitToMenu();
    }
}