using UnityEngine;
using UnityEngine.AI;

public class FleeingState : EnemyState
{
    private float _fleeDistance = 15f;

    public FleeingState(BaseEnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        ResetAllBools();
        _animator.SetBool("IsChasing", true);

        if (_agent.isActiveAndEnabled)
        {
            _agent.isStopped = false;
            _agent.speed = _stateMachine.ChaseSpeed * 1.5f;
        }
    }

    public override void Update()
    {
        if (_stateMachine.Player == null) return;

        Vector3 directionFromPlayer = (_transform.position - _stateMachine.Player.position).normalized;
        Vector3 fleeTargetPos = _transform.position + directionFromPlayer * _fleeDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeTargetPos, out hit, 5f, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }

        float distToPlayer = Vector3.Distance(_transform.position, _stateMachine.Player.position);

        // Если убежали достаточно далеко - возвращаемся к патрулю
        if (distToPlayer > _stateMachine.ChaseRange + 7f)
        {
            // Используем публичный метод вместо рефлексии
            _stateMachine.ResetFleeing();
            _stateMachine.ChangeState(_stateMachine.PatrolState);
        }
    }

    public override void Exit()
    {
        _animator.SetBool("IsChasing", false);
    }
}