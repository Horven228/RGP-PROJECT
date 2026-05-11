using UnityEngine;

public class MageAttackingState : EnemyState
{
    private float _timer;
    private bool _hasSpawned;
    private MageStateMachine _mageMachine;
    private BossWizardStateMachine _bossMachine;

    public MageAttackingState(MageStateMachine stateMachine) : base(stateMachine)
    {
        _mageMachine = stateMachine;
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

        // Проверяем, является ли этот маг боссом
        _bossMachine = _stateMachine as BossWizardStateMachine;
    }

    public override void Update()
    {
        _stateMachine.LookAtPlayer();
        _timer -= Time.deltaTime;

        if (!_hasSpawned && _timer <= _stateMachine.AttackDuration * 0.5f)
        {
            if (_bossMachine != null)
            {
                _bossMachine.PerformNormalAttack();
            }
            else if (_mageMachine != null)
            {
                _mageMachine.SpawnMagicBall();
            }
            _hasSpawned = true;
        }

        if (_timer <= 0)
        {
            _stateMachine.ChangeState(new ChasingState(_stateMachine));
        }
    }

    public override void Exit()
    {
        _animator.SetBool("IsAttacking", false);
    }
}