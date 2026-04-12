using UnityEngine;
using UnityEngine.UI;

// VIEW (Визуальное отображение кнопок)
public class SaveLoadView : MonoBehaviour
{
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;

    public void Initialize(SaveLoadController controller)
    {
        _saveButton.onClick.AddListener(controller.OnSaveClicked);
        _loadButton.onClick.AddListener(controller.OnLoadClicked);
    }
}

// CONTROLLER (Связующее звено)
public class SaveLoadController
{
    private readonly SaveLoadInteractor _interactor;
    private readonly PlayerController _player;
    private readonly PlayerHealth _pHealth;

    public SaveLoadController(SaveLoadInteractor interactor, PlayerController player, PlayerHealth pHealth)
    {
        _interactor = interactor;
        _player = player;
        _pHealth = pHealth;
    }

    public void OnSaveClicked() => _interactor.SaveGame(_player, _pHealth);
    public void OnLoadClicked() => _interactor.LoadGame(_player, _pHealth);
}