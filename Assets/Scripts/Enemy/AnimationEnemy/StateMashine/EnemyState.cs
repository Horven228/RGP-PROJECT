using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyState
{
    protected BaseEnemyStateMachine _stateMachine;
    protected Animator _animator;
    protected Transform _transform;
    protected NavMeshAgent _agent;

    // Конструктор теперь принимает базовый класс
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

    // Метод для очистки всех анимационных булов
    protected void ResetAllBools()
    {
        if (_animator == null) return;

        _animator.SetBool("IsPatroling", false);
        _animator.SetBool("IsChasing", false);
        _animator.SetBool("IsAttacking", false);

        // Используем Try-Catch или проверку параметров, 
        // если у мага нет параметра IsStrongAttacking
        _animator.SetBool("IsStrongAttacking", false);
    }
}