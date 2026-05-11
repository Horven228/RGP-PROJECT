using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyStateRestorer : MonoBehaviour
{
    [Header("Компоненты врага")]
    [SerializeField] private EnemyHealth _enemyHealth;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Collider _hitCollider;
    [SerializeField] private Animator _animator;
    [SerializeField] private BaseEnemyStateMachine _stateMachine;

    [Header("Настройки")]
    [SerializeField] private int _maxHealth = 100;

    private IHealthUI _healthUI;
    private float _defaultAgroDuration = 10f;

    private void Awake()
    {
        if (_enemyHealth == null) _enemyHealth = GetComponent<EnemyHealth>();
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();
        if (_hitCollider == null) _hitCollider = GetComponent<Collider>();
        if (_animator == null) _animator = GetComponentInChildren<Animator>();
        if (_stateMachine == null) _stateMachine = GetComponent<BaseEnemyStateMachine>();

        if (_enemyHealth != null)
            _maxHealth = _enemyHealth.MaxHealth;

        _healthUI = GetComponent<IHealthUI>() ?? GetComponentInChildren<IHealthUI>();
    }

    public void RestoreState(int health, Vector3 position, bool isDead, bool isAgro = false, float agroRemainingTime = 0f)
    {
        if (isDead)
        {
            RestoreDeadEnemy();
            return;
        }

        gameObject.SetActive(true);
        StartCoroutine(RestoreLivingEnemyCoroutine(health, position, isAgro, agroRemainingTime));
    }

    private void RestoreDeadEnemy()
    {
        if (_enemyHealth != null)
            _enemyHealth.SetDeadFlag(true);

        gameObject.SetActive(false);
        Debug.Log($"[EnemyStateRestorer] Враг {gameObject.name} восстановлен как МЕРТВЫЙ");
    }

    private IEnumerator RestoreLivingEnemyCoroutine(int health, Vector3 position, bool isAgro, float agroRemainingTime)
    {
        if (_enemyHealth != null)
        {
            _enemyHealth.SetDeadFlag(false);
            _enemyHealth.SetHealth(health);
        }

        RestorePosition(position);

        if (_hitCollider != null)
            _hitCollider.enabled = true;

        RestoreHealthUI(health);

        if (_animator != null)
            _animator.SetBool("Death", false);

        yield return null;

        RestoreAI(isAgro, agroRemainingTime);

        Debug.Log($"[EnemyStateRestorer] Враг {gameObject.name} восстановлен. HP={health}, isAgro={isAgro}");
    }

    private void RestorePosition(Vector3 position)
    {
        if (_agent == null)
        {
            transform.position = position;
            return;
        }

        _agent.enabled = false;
        transform.position = position;
        _agent.enabled = true;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, 5f, NavMesh.AllAreas))
        {
            _agent.Warp(hit.position);
            transform.position = hit.position;
        }

        _agent.isStopped = false;
        _agent.ResetPath();

        if (_stateMachine != null)
        {
            _agent.speed = _stateMachine.PatrolSpeed;
        }
    }

    private void RestoreHealthUI(int currentHealth)
    {
        if (_healthUI != null)
        {
            _healthUI.Initialize(_maxHealth);
            _healthUI.UpdateHealth(currentHealth, _maxHealth);
            _healthUI.ShowHealthBar();
        }
    }

    private void RestoreAI(bool isAgro, float agroRemainingTime)
    {
        if (_stateMachine == null)
        {
            Debug.LogError($"[RestoreAI] StateMachine отсутствует у {gameObject.name}");
            return;
        }

        if (_agent == null || !_agent.isOnNavMesh)
        {
            Debug.LogError($"[RestoreAI] NavMeshAgent не готов у {gameObject.name}");
            return;
        }

        _stateMachine.enabled = true;
        _stateMachine.ResetAggro();

        // Для босса: сбрасываем состояние фазы
        if (_stateMachine is BossWizardStateMachine bossMachine)
        {
            bossMachine.ResetPhaseState();
            Debug.Log($"[RestoreAI] Босс: состояние фазы сброшено");
        }

        if (isAgro)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _stateMachine.Player = player.transform;
            }

            float agroDuration = agroRemainingTime > 0 ? agroRemainingTime : _defaultAgroDuration;
            _stateMachine.SetAgro(true, agroDuration);
            _stateMachine.ChangeState(new ChasingState(_stateMachine));
            Debug.Log($"[RestoreAI] {gameObject.name} восстановлен в режиме АГРЕССИИ");
        }
        else
        {
            if (_stateMachine.PatrolPoints != null && _stateMachine.PatrolPoints.Count > 0)
            {
                _agent.ResetPath();
                Vector3 firstPoint = _stateMachine.PatrolPoints[0].position;
                _agent.SetDestination(firstPoint);
            }

            _stateMachine.ChangeState(new PatrollingState(_stateMachine));
            Debug.Log($"[RestoreAI] {gameObject.name} восстановлен в режиме ПАТРУЛЯ");
        }
    }
}