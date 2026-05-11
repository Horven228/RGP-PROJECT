using UnityEngine;

public class PatrollingState : EnemyState
{
    private bool _reachedPoint = false;

    public PatrollingState(BaseEnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        ResetAllBools();

        if (_agent != null && _agent.isActiveAndEnabled)
        {
            _agent.ResetPath();
            _agent.isStopped = false;
            _agent.speed = _stateMachine.PatrolSpeed;
        }

        _reachedPoint = false;

        if (HasParameter("IsPatroling"))
            _animator.SetBool("IsPatroling", true);
    }

    public override void Update()
    {
        // Проверка: если агент не готов - выходим
        if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
        {
            return;
        }

        if (_stateMachine.Player != null)
        {
            float dist = Vector3.Distance(_transform.position, _stateMachine.Player.position);

            bool isPeaceful = GameModeManager.Instance != null &&
                              GameModeManager.Instance.CurrentMode == GameMode.Peaceful;

            if (isPeaceful)
            {
                if (_stateMachine.IsBossEnemy && _stateMachine.IsAgro)
                {
                    _stateMachine.ChangeState(new ChasingState(_stateMachine));
                    return;
                }
            }
            else
            {
                if (dist < _stateMachine.ChaseRange || _stateMachine.IsAgro)
                {
                    _stateMachine.ChangeState(new ChasingState(_stateMachine));
                    return;
                }
            }
        }

        if (_stateMachine.PatrolPoints.Count == 0)
        {
            return;
        }

        if (!_reachedPoint)
        {
            Vector3 targetPoint = _stateMachine.PatrolPoints[_stateMachine.CurrentPatrolIndex].position;
            _agent.SetDestination(targetPoint);
        }

        if (!_agent.pathPending && _agent.remainingDistance < 0.6f)
        {
            if (!_reachedPoint)
            {
                _reachedPoint = true;

                int nextIndex = GetRandomPatrolIndex();
                _stateMachine.CurrentPatrolIndex = nextIndex;

                _stateMachine.ChangeState(new IdleState(_stateMachine));
            }
        }
    }

    public override void Exit()
    {
        if (HasParameter("IsPatroling"))
            _animator.SetBool("IsPatroling", false);
    }

    private int GetRandomPatrolIndex()
    {
        if (_stateMachine.PatrolPoints.Count <= 1)
            return 0;

        int newIndex;
        do
        {
            newIndex = Random.Range(0, _stateMachine.PatrolPoints.Count);
        }
        while (newIndex == _stateMachine.CurrentPatrolIndex && _stateMachine.PatrolPoints.Count > 1);

        return newIndex;
    }

    private bool HasParameter(string paramName)
    {
        if (_animator == null) return false;

        foreach (var param in _animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }
}