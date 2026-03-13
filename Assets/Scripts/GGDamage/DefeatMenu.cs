using UnityEngine;
using UnityEngine.SceneManagement;

public class DefeatMenu : MonoBehaviour
{

    // Метод для кнопки рестарт
    public void RestartLevel()
    {
        Time.timeScale = 1f;

        // Возвращаем блокировку курсора (при перезагрузке сцены это не обязательно,
        // но оставим для порядка)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}