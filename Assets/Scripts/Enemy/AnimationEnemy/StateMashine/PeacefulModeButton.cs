using UnityEngine;
using UnityEngine.UI;

public class PeacefulModeButton : MonoBehaviour
{
    [Header("Изображения для режимов")]
    [SerializeField] private Sprite _normalSprite;     // Красная картинка
    [SerializeField] private Sprite _peacefulSprite;   // Зеленая картинка

    private Image _buttonImage;
    private bool _isPeacefulMode = false;

    private void Start()
    {
        // Автоматически получаем компонент Image на кнопке
        _buttonImage = GetComponent<Image>();

        if (_buttonImage == null)
        {
            Debug.LogError("На кнопке нет компонента Image!");
            return;
        }

        LoadMode();
        UpdateButtonImage();

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(ToggleMode);
        }
    }

    public void ToggleMode()
    {
        _isPeacefulMode = !_isPeacefulMode;

        GameMode newMode = _isPeacefulMode ? GameMode.Peaceful : GameMode.Normal;

        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.CurrentMode = newMode;
        }

        SaveMode();
        UpdateButtonImage();
    }

    private void UpdateButtonImage()
    {
        if (_buttonImage != null)
        {
            _buttonImage.sprite = _isPeacefulMode ? _peacefulSprite : _normalSprite;
        }
    }

    private void SaveMode()
    {
        PlayerPrefs.SetInt("GameMode", _isPeacefulMode ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadMode()
    {
        _isPeacefulMode = PlayerPrefs.GetInt("GameMode", 0) == 1;

        if (GameModeManager.Instance != null)
        {
            GameMode mode = _isPeacefulMode ? GameMode.Peaceful : GameMode.Normal;
            GameModeManager.Instance.CurrentMode = mode;
        }
    }
}