using UnityEngine;

public class AttackingState : EnemyState
{
    private float _timer;

    public AttackingState(BaseEnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        if (_agent.isActiveAndEnabled) _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

        ResetAllBools();
        _animator.SetBool("IsAttacking", true);

        _timer = _stateMachine.AttackDuration;
        _stateMachine.LastAttackTime = Time.time;
    }

    public override void Update()
    {
        _stateMachine.LookAtPlayer();
        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {

            _stateMachine.ChangeState(_stateMachine.ChaseState);
        }
    }

    public override void Exit()
    {
        _animator.SetBool("IsAttacking", false);
    }
}