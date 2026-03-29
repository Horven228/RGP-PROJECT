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

    // Компоненты
    private CharacterController _characterController;
    private Animator _animator;
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _runAction;
    private InputAction _attackAction;
    private InputAction _magicAction;

    // Переменные состояния
    private Vector3 _velocity;
    private bool _isGrounded;
    private float _cameraPitch = 0f;
    private Vector2 _currentLookInput;
    private Vector2 _lookVelocity;
    private Vector2 _currentMoveInput;
    private bool _isRunning;

    // Переменные для атаки
    private bool _isAttacking;
    private float _attackTimer;

    // Переменные для магии
    private float _magicCooldownTimer = 0f;
    private bool _magicOnCooldown = false;
    private MagicCooldownUI _magicCooldownUI;

    // Hash для анимационных параметров
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
        if (_cameraHolder == null)
        {
            _cameraHolder = GetComponentInChildren<Camera>()?.transform;
            if (_cameraHolder == null)
            {
                Debug.LogError("[PlayerController] CameraHolder не найден!");
                enabled = false;
                return;
            }
        }

        if (_magicSpawnPoint == null)
        {
            _magicSpawnPoint = transform;
        }

        _magicCooldownUI = FindObjectOfType<MagicCooldownUI>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
            if (_magicCooldownTimer <= 0)
            {
                _magicOnCooldown = false;
            }
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
        float groundCheckDistance = 0.2f;
        _isGrounded = Physics.Raycast(ray, groundCheckDistance + 0.1f, _groundMask);
    }

    private void HandleMovement()
    {
        _currentMoveInput = _moveAction.ReadValue<Vector2>();

        if (_isAttacking) return;

        if (_currentMoveInput.magnitude > 0.1f)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection = (forward * _currentMoveInput.y + right * _currentMoveInput.x).normalized;

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

            if (_smoothCamera)
            {
                _currentLookInput = Vector2.SmoothDamp(
                    _currentLookInput,
                    lookInput,
                    ref _lookVelocity,
                    _cameraSmoothTime
                );
            }
            else
            {
                _currentLookInput = lookInput;
            }

            float yawDelta = _currentLookInput.x;
            transform.Rotate(Vector3.up * yawDelta);

            float pitchDelta = _currentLookInput.y * (_invertY ? 1f : -1f);
            _cameraPitch += pitchDelta;
            _cameraPitch = Mathf.Clamp(_cameraPitch, -_cameraPitchLimit, _cameraPitchLimit);

            _cameraHolder.localEulerAngles = Vector3.right * _cameraPitch;
        }
    }

    private void ApplyGravity()
    {
        if (!_isGrounded)
        {
            _velocity.y += _gravity * Time.deltaTime;
        }
        else if (_velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        _characterController.Move(_velocity * Time.deltaTime);
    }

    private void HandleRun()
    {
        if (_runAction == null) return;
        if (_isAttacking) return;

        _isRunning = _runAction.IsPressed() && _currentMoveInput.magnitude > 0.1f;
    }

    private void HandleAttack()
    {
        if (_attackAction == null) return;

        if (_attackAction.WasPressedThisFrame() && !_isAttacking)
        {
            _isAttacking = true;
            _attackTimer = _attackDuration;
            _animator.SetBool(IsAttackingHash, true);
            _isRunning = false;
        }
    }

    private void HandleMagic()
    {
        if (_magicAction == null || _magicEffectPrefab == null) return;

        if (_magicAction.WasPressedThisFrame() && !_magicOnCooldown)
        {
            Vector3 spawnPosition = _magicSpawnPoint != null ? _magicSpawnPoint.position : transform.position;
            Quaternion spawnRotation = _magicSpawnPoint != null ? _magicSpawnPoint.rotation : transform.rotation;

            GameObject magic = Instantiate(_magicEffectPrefab, spawnPosition, spawnRotation);

            _magicOnCooldown = true;
            _magicCooldownTimer = _magicCooldown;

            if (_magicCooldownUI != null)
            {
                _magicCooldownUI.TriggerCooldown();
            }

            Destroy(magic, 5f);
        }
    }

    private void UpdateAnimations()
    {
        if (_animator == null) return;

        if (_isAttacking)
        {
            _animator.SetBool(IsWalkingHash, false);
            _animator.SetBool(IsRunningHash, false);
            return;
        }

        bool isMoving = _currentMoveInput.magnitude > 0.1f;
        bool isRunning = _isRunning && isMoving;
        bool isWalking = isMoving && !isRunning;

        _animator.SetBool(IsWalkingHash, isWalking);
        _animator.SetBool(IsRunningHash, isRunning);
    }

    public void DisableControl()
    {
        this.enabled = false;
        if (_playerInput != null)
            _playerInput.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[PlayerController] Управление отключено");
    }

    public void EnableControl()
    {
        this.enabled = true;
        if (_playerInput != null)
            _playerInput.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("[PlayerController] Управление включено");
    }

    private void OnEnable()
    {
        if (_playerInput != null)
        {
            _playerInput.ActivateInput();
        }
    }

    private void OnDisable()
    {
        if (_playerInput != null)
        {
            _playerInput.DeactivateInput();
        }
    }
}