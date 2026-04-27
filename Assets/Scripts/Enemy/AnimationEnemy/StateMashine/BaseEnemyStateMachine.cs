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
    public Transform Player { get; private set; }

    protected EnemyState _currentState;
    public float LastAttackTime { get; set; }

    public PatrollingState PatrolState { get; protected set; }
    public ChasingState ChaseState { get; protected set; }
    public FleeingState FleeState { get; protected set; }
    public DeadState DeadStateObj { get; protected set; }

    protected bool _alreadyFleeing = false;
    private bool _isPeacefulMode = false;

    public virtual bool IsBossEnemy => false;

    protected virtual void Awake()
    {
        FindPlayerByTag();

        if (Health == null) Health = GetComponent<EnemyHealth>();
        if (Animator == null) Animator = GetComponent<Animator>();
        if (Agent == null) Agent = GetComponent<NavMeshAgent>();

        PatrolState = new PatrollingState(this);
        ChaseState = new ChasingState(this);
        FleeState = new FleeingState(this);
        DeadStateObj = new DeadState(this);

        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.OnGameModeChanged += OnGameModeChanged;
            OnGameModeChanged(GameModeManager.Instance.CurrentMode);
        }
    }

    protected virtual void Start()
    {
        if (Health != null) Health.OnTakeDamage += HandleTakeDamage;
        ChangeState(PatrolState);
    }

    private void OnGameModeChanged(GameMode mode)
    {
        _isPeacefulMode = (mode == GameMode.Peaceful);

        if (_isPeacefulMode && IsAgro)
        {
            IsAgro = false;
            _agroTimer = 0f;
            if (_currentState == ChaseState)
            {
                ChangeState(PatrolState);
            }
        }
    }

    protected virtual void Update()
    {
        if (Health != null && Health.IsDead && _currentState != DeadStateObj)
        {
            ChangeState(DeadStateObj);
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
        // МИРНЫЙ РЕЖИМ
        if (_isPeacefulMode)
        {
            // Если это босс - агрится при ударе
            if (IsBossEnemy)
            {
                Debug.Log($"[{gameObject.name}] Босс в мирном режиме получил урон - АГРИТСЯ!");
                if (Player == null) FindPlayerByTag();
                if (Player == null) return;

                IsAgro = true;
                _agroTimer = _agroDuration;

                if (_currentState != ChaseState && _currentState != DeadStateObj)
                {
                    if (_currentState == FleeState)
                    {
                        _alreadyFleeing = false;
                    }
                    ChangeState(ChaseState);
                }
            }
            // Обычный моб в мирном режиме - не агрится, но проверяем здоровье для побега
            else
            {
                Debug.Log($"[{gameObject.name}] Обычный моб в мирном режиме - получил урон, но не агрится");
                // Проверяем, не пора ли убегать при низком здоровье
                CheckHealthForFlee();
            }
            return;
        }

        // ОБЫЧНЫЙ РЕЖИМ - все агрятся
        if (Health.IsDead || _alreadyFleeing) return;

        if (Player == null) FindPlayerByTag();
        if (Player == null) return;

        IsAgro = true;
        _agroTimer = _agroDuration;

        if (_currentState != ChaseState && _currentState != DeadStateObj)
        {
            if (_currentState == FleeState)
            {
                _alreadyFleeing = false;
            }
            ChangeState(ChaseState);
        }
    }

    private void CheckHealthForFlee()
    {
        // Проверяем только если живы и еще не убегаем
        if (_alreadyFleeing || Health == null || Health.IsDead) return;

        float hpPercent = (float)Health.CurrentHealth / 100f;

        if (hpPercent <= 0.3f)
        {
            // Боссы не убегают
            if (!IsBossEnemy)
            {
                Debug.Log($"[{gameObject.name}] Здоровье ниже 30% - УБЕГАЮ!");
                _alreadyFleeing = true;
                IsAgro = false;
                ChangeState(FleeState);
            }
        }
    }

    public void ChangeState(EnemyState newState)
    {
        if (_currentState == newState) return;
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

    // Публичный метод для сброса флага убегания
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
}