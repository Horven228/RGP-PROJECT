using UnityEngine;
using UnityEngine.UI;

public class SaveLoadView : MonoBehaviour
{
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;

    private SaveLoadController _controller;

    public void Initialize(SaveLoadController controller)
    {
        _controller = controller;

        if (_saveButton != null)
            _saveButton.onClick.AddListener(() => _controller?.OnSaveClicked());

        if (_loadButton != null)
            _loadButton.onClick.AddListener(() => _controller?.OnLoadClicked());

        Debug.Log("[SaveLoadView] Инициализирован");
    }

    private void OnDestroy()
    {
        if (_saveButton != null)
            _saveButton.onClick.RemoveAllListeners();

        if (_loadButton != null)
            _loadButton.onClick.RemoveAllListeners();
    }
}