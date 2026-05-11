using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    // СОБЫТИЕ ДЛЯ СПАВНЕРА БОССА
    public event System.Action<int> OnScoreChanged;
    
    private static ScoreManager _instance;
    public static ScoreManager Instance => _instance;

    [Header("Настройки")]
    [SerializeField] private int _scorePerKill = 1;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _scoreText;

    private int _currentScore = 0;
    private bool _isFirstLoad = true;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[ScoreManager] Awake - Instance создан");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Подписываемся на событие смерти врага
        EnemyHealth.OnEnemyDeath += OnEnemyKilled;

        // Подписываемся на событие загрузки сцены
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Обновляем UI
        UpdateScoreUI();

        Debug.Log("[ScoreManager] Start - Инициализирован");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // При загрузке новой сцены (кроме самой первой) сбрасываем очки
        if (!_isFirstLoad)
        {
            ResetScore();
            Debug.Log("[ScoreManager] Сцена загружена, очки сброшены");
        }
        else
        {
            _isFirstLoad = false;
        }

        // Обновляем UI (находим новый текст на новой сцене)
        UpdateScoreUI();
    }

    private void OnEnable()
    {
        // При каждой активации обновляем UI
        UpdateScoreUI();
    }

    private void OnDestroy()
    {
        if (EnemyHealth.OnEnemyDeath != null)
            EnemyHealth.OnEnemyDeath -= OnEnemyKilled;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnEnemyKilled(GameObject enemy)
    {
        AddScore(_scorePerKill);
        Debug.Log($"[ScoreManager] Убит {enemy.name}. Очки: {_currentScore}");
    }

    public void AddScore(int amount)
    {
        _currentScore += amount;
        UpdateScoreUI();
        
        // ВЫЗЫВАЕМ СОБЫТИЕ ДЛЯ БОССА
        OnScoreChanged?.Invoke(_currentScore);
        
        Debug.Log($"[ScoreManager] Добавлено {amount} очков. Всего: {_currentScore}");
    }

    public void SetScore(int score)
    {
        _currentScore = score;
        UpdateScoreUI();
        
        // ВЫЗЫВАЕМ СОБЫТИЕ ДЛЯ БОССА
        OnScoreChanged?.Invoke(_currentScore);
        
        Debug.Log($"[ScoreManager] Очки установлены: {_currentScore}");
    }

    public int GetScore()
    {
        return _currentScore;
    }

    public void ResetScore()
    {
        _currentScore = 0;
        UpdateScoreUI();
        
        // ВЫЗЫВАЕМ СОБЫТИЕ ДЛЯ БОССА
        OnScoreChanged?.Invoke(_currentScore);
        
        Debug.Log("[ScoreManager] Очки сброшены");
    }

    private void UpdateScoreUI()
    {
        // Если ссылка потеряна, ищем текст на сцене
        if (_scoreText == null)
        {
            _scoreText = FindObjectOfType<TextMeshProUGUI>();
        }

        if (_scoreText != null)
        {
            _scoreText.text = $"Очки: {_currentScore}";
        }
    }
}