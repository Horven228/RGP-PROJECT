using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public abstract class BaseEnemyStateMachine : MonoBehaviour
{
    [Header("Основные компоненты")]
    public Animator Animator;
    public NavMeshAgent Agent;
    public EnemyHealth Health;

    [Header("Настройки дистанций")]
    public float PatrolSpeed = 2f;
    public float ChaseSpeed = 4.5f;
    public float ChaseRange = 10f;
    public float AttackRange = 2f;

    [Header("Настройки Агро")]
    [SerializeField] private float _agroDuration = 10f;
    public bool IsAgro { get; private set; }
    private float _agroTimer;

    [Header("Настройки времени")]
    public float AttackCooldown = 2f;
    public float AttackDuration = 1.2f;

    public List<Transform> PatrolPoints;
    public Transform Player { get; set; }

    private EnemyState _currentState;
    public float LastAttackTime { get; set; }

    public int CurrentPatrolIndex { get; set; } = 0;

    protected bool _alreadyFleeing = false;
    private bool _isPeacefulMode = false;

    public virtual bool IsBossEnemy => false;

    /// <summary>
    /// Устанавливает состояние агрессии
    /// </summary>
    public void SetAgro(bool agro, float duration = 10f)
    {
        IsAgro = agro;
        if (agro)
        {
            _agroTimer = duration;
        }
        else
        {
            _agroTimer = 0f;
        }

        Debug.Log($"[BaseEnemyStateMachine] {gameObject.name} SetAgro: {agro}, duration: {duration}");
    }

    /// <summary>
    /// Возвращает оставшееся время агрессии (для сохранения)
    /// </summary>
    public float GetAgroRemainingTime()
    {
        return IsAgro ? _agroTimer : 0f;
    }

    protected virtual void Awake()
    {
        FindPlayerByTag();

        if (Health == null) Health = GetComponent<EnemyHealth>();
        if (Animator == null) Animator = GetComponent<Animator>();
        if (Agent == null) Agent = GetComponent<NavMeshAgent>();

        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.OnGameModeChanged += OnGameModeChanged;
            OnGameModeChanged(GameModeManager.Instance.CurrentMode);
        }
    }

    protected virtual void Start()
    {
        if (Health != null) Health.OnTakeDamage += HandleTakeDamage;

        SetRandomStartPatrolIndex();

        ChangeState(new PatrollingState(this));
    }

    private void OnGameModeChanged(GameMode mode)
    {
        _isPeacefulMode = (mode == GameMode.Peaceful);

        if (_isPeacefulMode && IsAgro)
        {
            IsAgro = false;
            _agroTimer = 0f;
            if (_currentState is ChasingState)
            {
                ChangeState(new PatrollingState(this));
            }
        }
    }

    public void ResetAggro()
    {
        IsAgro = false;
        _agroTimer = 0f;
        _alreadyFleeing = false;
    }

    protected virtual void Update()
    {
        if (Health != null && Health.IsDead && !(_currentState is DeadState))
        {
            ChangeState(new DeadState(this));
            return;
        }

        if (IsAgro)
        {
            _agroTimer -= Time.deltaTime;
            if (_agroTimer <= 0) IsAgro = false;
        }

        CheckHealthForFlee();
        if (_currentState != null) _currentState.Update();
    }

    private void FindPlayerByTag()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) Player = playerObj.transform;
    }

    public void TriggerAggro(int damage, GameObject attacker)
    {
        HandleTakeDamage(damage, attacker);
    }

    private void HandleTakeDamage(int damage, GameObject attacker)
    {
        if (_isPeacefulMode)
        {
            if (IsBossEnemy)
            {
                Debug.Log($"[{gameObject.name}] Босс в мирном режиме получил урон - АГРИТСЯ!");
                if (Player == null) FindPlayerByTag();
                if (Player == null) return;

                IsAgro = true;
                _agroTimer = _agroDuration;

                if (!(_currentState is ChasingState) && !(_currentState is DeadState))
                {
                    if (_currentState is FleeingState)
                    {
                        _alreadyFleeing = false;
                    }
                    ChangeState(new ChasingState(this));
                }
            }
            else
            {
                Debug.Log($"[{gameObject.name}] Обычный моб в мирном режиме - получил урон, но не агрится");
                CheckHealthForFlee();
            }
            return;
        }

        if (Health.IsDead || _alreadyFleeing) return;

        if (Player == null) FindPlayerByTag();
        if (Player == null) return;

        IsAgro = true;
        _agroTimer = _agroDuration;

        if (!(_currentState is ChasingState) && !(_currentState is DeadState))
        {
            if (_currentState is FleeingState)
            {
                _alreadyFleeing = false;
            }
            ChangeState(new ChasingState(this));
        }
    }

    private void CheckHealthForFlee()
    {
        if (_alreadyFleeing || Health == null || Health.IsDead) return;

        if (_currentState is IdleState) return;

        float hpPercent = (float)Health.CurrentHealth / Health.MaxHealth;

        if (hpPercent <= 0.3f)
        {
            if (!IsBossEnemy)
            {
                Debug.Log($"[{gameObject.name}] Здоровье ниже 30% - УБЕГАЮ!");
                _alreadyFleeing = true;
                IsAgro = false;
                ChangeState(new FleeingState(this));
            }
        }
    }

    public void ChangeState(EnemyState newState)
    {
        if (_currentState != null && _currentState.GetType() == newState.GetType()) return;

        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
    }

    public void LookAtPlayer()
    {
        if (Player == null) return;
        Vector3 dir = Player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 8f);
    }

    public void ResetFleeing()
    {
        _alreadyFleeing = false;
    }

    private void OnDestroy()
    {
        if (Health != null) Health.OnTakeDamage -= HandleTakeDamage;
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.OnGameModeChanged -= OnGameModeChanged;
        }
    }

    public void SetRandomStartPatrolIndex()
    {
        if (PatrolPoints.Count > 0)
        {
            CurrentPatrolIndex = Random.Range(0, PatrolPoints.Count);
        }
    }
}