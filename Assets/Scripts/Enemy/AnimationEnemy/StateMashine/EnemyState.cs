using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyState
{
    protected BaseEnemyStateMachine _stateMachine;
    protected Animator _animator;
    protected Transform _transform;
    protected NavMeshAgent _agent;

    public EnemyState(BaseEnemyStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _animator = stateMachine.Animator;
        _transform = stateMachine.transform;
        _agent = stateMachine.Agent;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();

    protected void ResetAllBools()
    {
        if (_animator == null) return;

        if (HasParameter("IsPatroling")) _animator.SetBool("IsPatroling", false);
        if (HasParameter("IsChasing")) _animator.SetBool("IsChasing", false);
        if (HasParameter("IsAttacking")) _animator.SetBool("IsAttacking", false);
        if (HasParameter("IsStrongAttacking")) _animator.SetBool("IsStrongAttacking", false);
        if (HasParameter("IsIdle")) _animator.SetBool("IsIdle", false);
    }

    protected bool HasParameter(string paramName)
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