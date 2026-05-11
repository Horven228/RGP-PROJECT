using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    [Header("Настройки врага")]
    [SerializeField] private string _victoryEnemyName = "Boss"; // Имя босса

    [Header("UI Меню")]
    [SerializeField] private GameObject _victoryMenu;

    [Header("Музыка")]
    [SerializeField] private AudioClip _victoryMusic;

    private AudioSource _audioSource;
    private bool _isVictory = false;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        if (_victoryMenu != null)
            _victoryMenu.SetActive(false);

        EnemyHealth.OnEnemyDeath += OnEnemyKilled;
    }

    private void OnDestroy()
    {
        EnemyHealth.OnEnemyDeath -= OnEnemyKilled;
    }

    private void OnEnemyKilled(GameObject enemy)
    {
        if (_isVictory) return;

        // Проверяем по имени (НЕ по тегу)
        if (enemy.name.Contains(_victoryEnemyName))
        {
            _isVictory = true;
            StartCoroutine(ShowVictoryMenu());
        }
    }

    private System.Collections.IEnumerator ShowVictoryMenu()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1f);

        if (_victoryMenu != null)
        {
            _victoryMenu.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (_victoryMusic != null && _audioSource != null)
        {
            _audioSource.clip = _victoryMusic;
            _audioSource.loop = false;
            _audioSource.Play();
        }

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
            player.DisableControl();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        // Сбрасываем очки
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;

        // Сбрасываем очки
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();

        SceneManager.LoadScene("MainMenu");
    }
}