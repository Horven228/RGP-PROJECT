using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MagicCooldownUI : MonoBehaviour
{
    [Header("Иконки")]
    [SerializeField] private Image _magicIcon;
    [SerializeField] private Sprite _readyIcon; // Картинка когда магия готова
    [SerializeField] private Sprite _cooldownIcon; // Картинка когда магия на перезарядке

    [Header("Настройки кулдауна")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private string _magicActionName = "Magic";
    [SerializeField] private float _cooldownDuration = 5f;

    private InputAction _magicAction;
    private float _cooldownTimer = 0f;
    private bool _isOnCooldown = false;

    // Публичные свойства для доступа из PlayerController
    public bool IsOnCooldown => _isOnCooldown;
    public float CooldownRemaining => _cooldownTimer;

    private void Start()
    {
        // Находим InputAction если не назначен
        if (_playerInput == null)
            _playerInput = FindObjectOfType<PlayerInput>();

        if (_playerInput != null)
            _magicAction = _playerInput.actions[_magicActionName];

        // Находим компонент если не назначен
        if (_magicIcon == null)
            _magicIcon = GetComponent<Image>();

        // Устанавливаем начальное состояние (иконка готова к использованию)
        if (_readyIcon != null)
            _magicIcon.sprite = _readyIcon;
    }

    private void Update()
    {
        if (_isOnCooldown)
        {
            _cooldownTimer -= Time.deltaTime;

            // Проверяем окончание кулдауна
            if (_cooldownTimer <= 0)
            {
                _isOnCooldown = false;
                // Возвращаем иконку готовности
                if (_readyIcon != null)
                    _magicIcon.sprite = _readyIcon;
            }
        }

        // Проверяем нажатие кнопки магии
        if (_magicAction != null && _magicAction.WasPressedThisFrame() && !_isOnCooldown)
        {
            StartCooldown();
        }
    }

    public void StartCooldown()
    {
        _isOnCooldown = true;
        _cooldownTimer = _cooldownDuration;

        // Меняем на иконку кулдауна
        if (_cooldownIcon != null)
            _magicIcon.sprite = _cooldownIcon;
    }

    public void TriggerCooldown()
    {
        StartCooldown();
    }

    // Восстановление состояния кулдауна при загрузке
    public void RestoreCooldown(float remainingTime)
    {
        if (remainingTime > 0)
        {
            _isOnCooldown = true;
            _cooldownTimer = remainingTime;
            if (_cooldownIcon != null)
                _magicIcon.sprite = _cooldownIcon;
        }
        else
        {
            _isOnCooldown = false;
            _cooldownTimer = 0f;
            if (_readyIcon != null)
                _magicIcon.sprite = _readyIcon;
        }
    }
}