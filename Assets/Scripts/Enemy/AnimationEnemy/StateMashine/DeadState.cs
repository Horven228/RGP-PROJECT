using UnityEngine;

public class DeadState : EnemyState
{
    public DeadState(BaseEnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        ResetAllBools();
        if (_agent.isActiveAndEnabled) _agent.isStopped = true;
        _agent.enabled = false;
        _animator.SetBool("Death", true);
    }

    public override void Update() { }
    public override void Exit() { }
}