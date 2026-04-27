using UnityEngine;

public class PatrollingState : EnemyState
{
    private int _index = 0;

    public PatrollingState(BaseEnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        ResetAllBools();
        _agent.speed = _stateMachine.PatrolSpeed;
        _agent.isStopped = false;
        _animator.SetBool("IsPatroling", true);
    }

    public override void Update()
    {
        if (_stateMachine.Player != null)
        {
            float dist = Vector3.Distance(_transform.position, _stateMachine.Player.position);

            // Проверяем мирный режим
            bool isPeaceful = GameModeManager.Instance != null &&
                              GameModeManager.Instance.CurrentMode == GameMode.Peaceful;

            if (isPeaceful)
            {
                // В мирном режиме: босс не агрится просто так, только если IsAgro = true
                // Обычные мобы вообще не агрятся
                if (_stateMachine.IsBossEnemy)
                {
                    // Босс агрится только если IsAgro уже true (после удара)
                    if (_stateMachine.IsAgro)
                    {
                        _stateMachine.ChangeState(_stateMachine.ChaseState);
                        return;
                    }
                }
                // Обычные мобы - не агрятся вообще
            }
            else
            {
                // Обычный режим - агримся при приближении
                if (dist < _stateMachine.ChaseRange || _stateMachine.IsAgro)
                {
                    _stateMachine.ChangeState(_stateMachine.ChaseState);
                    return;
                }
            }
        }

        if (_stateMachine.PatrolPoints.Count == 0) return;
        _agent.SetDestination(_stateMachine.PatrolPoints[_index].position);

        if (!_agent.pathPending && _agent.remainingDistance < 0.6f)
            _index = (_index + 1) % _stateMachine.PatrolPoints.Count;
    }

    public override void Exit() => _animator.SetBool("IsPatroling", false);
}