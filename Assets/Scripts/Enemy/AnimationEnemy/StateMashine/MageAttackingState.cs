using UnityEngine;

public class MageAttackingState : EnemyState
{
    private float _timer;
    private bool _hasSpawned;
    private MageStateMachine _mageMachine;

    public MageAttackingState(BaseEnemyStateMachine stateMachine) : base(stateMachine)
    {
        _mageMachine = (MageStateMachine)stateMachine;
    }

    public override void Enter()
    {
        if (_agent.isActiveAndEnabled) _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

        ResetAllBools();
        _animator.SetBool("IsAttacking", true);

        _timer = _stateMachine.AttackDuration;
        _stateMachine.LastAttackTime = Time.time;
        _hasSpawned = false;
    }

    public override void Update()
    {
        _stateMachine.LookAtPlayer();
        _timer -= Time.deltaTime;

        // Спавним шар в середине анимации (когда таймер прошел половину)
        if (!_hasSpawned && _timer <= _stateMachine.AttackDuration * 0.5f)
        {
            _mageMachine.SpawnMagicBall();
            _hasSpawned = true;
        }

        if (_timer <= 0)
        {
            _stateMachine.ChangeState(_stateMachine.ChaseState);
        }
    }

    public override void Exit() => _animator.SetBool("IsAttacking", false);
}