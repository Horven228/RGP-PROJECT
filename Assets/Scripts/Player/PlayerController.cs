using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour, IPlayerController
{
    [Header("Настройки движения")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _runSpeed = 8f;
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private LayerMask _groundMask = -1;

    [Header("Настройки камеры")]
    [SerializeField] private Transform _cameraHolder;
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _cameraPitchLimit = 85f;
    [SerializeField] private bool _invertY = false;

    [Header("Плавность камеры")]
    [SerializeField] private bool _smoothCamera = true;
    [SerializeField] private float _cameraSmoothTime = 0.1f;

    [Header("Магический эффект")]
    [SerializeField] private GameObject _magicEffectPrefab;
    [SerializeField] private Transform _magicSpawnPoint;
    [SerializeField] private float _magicCooldown = 5f;

    [Header("Настройки атаки")]
    [SerializeField] private float _attackDuration = 1f;

    private CharacterController _characterController;
    private Animator _animator;
    private PlayerInput _playerInput;
    private InputAction _moveAction, _lookAction, _runAction, _attackAction, _magicAction;

    private Vector3 _velocity;
    private bool _isGrounded;
    private float _cameraPitch = 0f;
    private Vector2 _currentLookInput, _lookVelocity, _currentMoveInput;
    private bool _isRunning, _isAttacking;
    private float _attackTimer, _magicCooldownTimer = 0f;
    private bool _magicOnCooldown = false;
    private MagicCooldownUI _magicCooldownUI;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _playerInput = GetComponent<PlayerInput>();
        _moveAction = _playerInput.actions["Move"];
        _lookAction = _playerInput.actions["Look"];
        _runAction = _playerInput.actions["Run"];
        _attackAction = _playerInput.actions["Attack"];
        _magicAction = _playerInput.actions["Magic"];
    }

    private void Start()
    {
        if (_cameraHolder == null) _cameraHolder = GetComponentInChildren<Camera>()?.transform;
        if (_magicSpawnPoint == null) _magicSpawnPoint = transform;
        _magicCooldownUI = FindObjectOfType<MagicCooldownUI>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // МЕТОД ДЛЯ ТЕЛЕПОРТАЦИИ
    public void Teleport(Vector3 position)
    {
        _characterController.enabled = false;
        transform.position = position;
        _velocity = Vector3.zero;
        _characterController.enabled = true;
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleCameraLook();
        ApplyGravity();
        HandleRun();
        HandleAttack();
        HandleMagic();
        UpdateAnimations();

        if (_magicOnCooldown)
        {
            _magicCooldownTimer -= Time.deltaTime;
            if (_magicCooldownTimer <= 0) _magicOnCooldown = false;
        }

        if (_isAttacking)
        {
            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0)
            {
                _isAttacking = false;
                _animator.SetBool(IsAttackingHash, false);
            }
        }
    }

    private void HandleGroundCheck()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        _isGrounded = Physics.Raycast(ray, 0.3f, _groundMask);
    }

    private void HandleMovement()
    {
        _currentMoveInput = _moveAction.ReadValue<Vector2>();
        if (_isAttacking) return;
        if (_currentMoveInput.magnitude > 0.1f)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            forward.y = 0f; right.y = 0f;
            Vector3 moveDirection = (forward.normalized * _currentMoveInput.y + right.normalized * _currentMoveInput.x).normalized;
            float currentSpeed = _isRunning ? _runSpeed : _moveSpeed;
            _characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
    }

    private void HandleCameraLook()
    {
        Vector2 lookInput = _lookAction.ReadValue<Vector2>();
        if (lookInput.magnitude > 0.1f)
        {
            lookInput *= _mouseSensitivity * Time.deltaTime;
            _currentLookInput = _smoothCamera ? Vector2.SmoothDamp(_currentLookInput, lookInput, ref _lookVelocity, _cameraSmoothTime) : lookInput;
            transform.Rotate(Vector3.up * _currentLookInput.x);
            _cameraPitch += _currentLookInput.y * (_invertY ? 1f : -1f);
            _cameraPitch = Mathf.Clamp(_cameraPitch, -_cameraPitchLimit, _cameraPitchLimit);
            _cameraHolder.localEulerAngles = Vector3.right * _cameraPitch;
        }
    }

    private void ApplyGravity()
    {
        if (!_isGrounded) _velocity.y += _gravity * Time.deltaTime;
        else if (_velocity.y < 0) _velocity.y = -2f;
        _characterController.Move(_velocity * Time.deltaTime);
    }

    private void HandleRun() { if (_runAction != null && !_isAttacking) _isRunning = _runAction.IsPressed() && _currentMoveInput.magnitude > 0.1f; }

    private void HandleAttack()
    {
        if (_attackAction != null && _attackAction.WasPressedThisFrame() && !_isAttacking)
        {
            _isAttacking = true;
            _attackTimer = _attackDuration;
            _animator.SetBool(IsAttackingHash, true);
            _isRunning = false;
        }
    }

    private void HandleMagic()
    {
        if (_magicAction != null && _magicEffectPrefab != null && _magicAction.WasPressedThisFrame() && !_magicOnCooldown)
        {
            Instantiate(_magicEffectPrefab, _magicSpawnPoint.position, _magicSpawnPoint.rotation);
            _magicOnCooldown = true;
            _magicCooldownTimer = _magicCooldown;
            if (_magicCooldownUI != null) _magicCooldownUI.TriggerCooldown();
        }
    }

    private void UpdateAnimations()
    {
        if (_animator == null) return;
        if (_isAttacking) { _animator.SetBool(IsWalkingHash, false); _animator.SetBool(IsRunningHash, false); return; }
        bool isMoving = _currentMoveInput.magnitude > 0.1f;
        _animator.SetBool(IsWalkingHash, isMoving && !_isRunning);
        _animator.SetBool(IsRunningHash, isMoving && _isRunning);
    }

    public void DisableControl()
    {
        // Сначала деактивируем ввод, потом выключаем скрипт
        if (_playerInput != null) _playerInput.DeactivateInput();
        this.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void EnableControl()
    {
        // Сначала включаем скрипт, потом активируем ввод
        this.enabled = true;
        if (_playerInput != null)
        {
            _playerInput.enabled = true;
            _playerInput.ActivateInput();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Методы для сохранения/загрузки кулдауна
    public float GetMagicCooldownRemaining()
    {
        // Приоритет отдаем UI, так как там хранится актуальный таймер
        if (_magicCooldownUI != null && _magicCooldownUI.IsOnCooldown)
            return _magicCooldownUI.CooldownRemaining;
        return _magicOnCooldown ? _magicCooldownTimer : 0f;
    }

    public bool IsMagicOnCooldown()
    {
        if (_magicCooldownUI != null)
            return _magicCooldownUI.IsOnCooldown;
        return _magicOnCooldown;
    }

    public void RestoreMagicCooldown(float remainingTime, bool onCooldown)
    {
        _magicOnCooldown = onCooldown;
        _magicCooldownTimer = remainingTime;

        // Восстанавливаем UI
        if (_magicCooldownUI != null)
        {
            _magicCooldownUI.RestoreCooldown(remainingTime);
        }
    }

    private void OnEnable() { if (_playerInput != null) _playerInput.ActivateInput(); }
    private void OnDisable() { if (_playerInput != null) _playerInput.DeactivateInput(); }
}