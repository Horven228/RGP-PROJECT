using UnityEngine;

public class StrongAttackingState : EnemyState
{
    private float _timer;
    private MeleeStateMachine _meleeMachine;

    public StrongAttackingState(MeleeStateMachine stateMachine) : base(stateMachine)
    {
        _meleeMachine = stateMachine;
    }

    public override void Enter()
    {
        if (_agent.isActiveAndEnabled) _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

        ResetAllBools();
        _animator.SetBool("IsStrongAttacking", true);

        _timer = _meleeMachine.StrongAttackDuration;
        _meleeMachine.LastStrongAttackTime = Time.time;
    }

    public override void Update()
    {
        _stateMachine.LookAtPlayer();
        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            _stateMachine.ChangeState(new ChasingState(_stateMachine));
        }
    }

    public override void Exit()
    {
        _animator.SetBool("IsStrongAttacking", false);
    }
}