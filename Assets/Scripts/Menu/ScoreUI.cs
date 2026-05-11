using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    private TextMeshProUGUI _scoreText;

    private void Start()
    {
        _scoreText = GetComponent<TextMeshProUGUI>();
        if (_scoreText == null)
            _scoreText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (ScoreManager.Instance != null && _scoreText != null)
        {
            _scoreText.text = $"Очки: {ScoreManager.Instance.GetScore()}";
        }
    }
}