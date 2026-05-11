using UnityEngine;

public class ChasingState : EnemyState
{
    private bool _isWaitingForAttack = false;

    public ChasingState(BaseEnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        ResetAllBools();
        _isWaitingForAttack = false;

        if (_stateMachine.Player != null)
        {
            float dist = Vector3.Distance(_transform.position, _stateMachine.Player.position);

            if (dist <= _stateMachine.AttackRange)
            {
                if (_agent.isActiveAndEnabled) _agent.isStopped = true;
                _animator.SetBool("IsChasing", false);
                _stateMachine.LookAtPlayer();
                return;
            }
        }

        _animator.SetBool("IsChasing", true);

        if (_agent.isActiveAndEnabled)
        {
            _agent.speed = _stateMachine.ChaseSpeed;
            _agent.isStopped = false;
        }
    }

    public override void Update()
    {
        if (_stateMachine.Player == null) return;

        float dist = Vector3.Distance(_transform.position, _stateMachine.Player.position);

        if (dist <= _stateMachine.AttackRange)
        {
            if (_agent.isActiveAndEnabled) _agent.isStopped = true;

            if (_animator.GetBool("IsChasing"))
            {
                _animator.SetBool("IsChasing", false);
            }

            _stateMachine.LookAtPlayer();

            if (!_isWaitingForAttack)
            {
                // ============ ДЛЯ БОССА-МАГА ============
                if (_stateMachine is BossWizardStateMachine bossWizard)
                {
                    if (bossWizard.CanPerformStrongAttack() && Random.value < 0.3f)
                    {
                        _isWaitingForAttack = true;
                        bossWizard.ChangeState(new BossWizardStrongAttackingState(bossWizard));
                        return;
                    }
                    else if (bossWizard.CanPerformNormalAttack())
                    {
                        _isWaitingForAttack = true;
                        bossWizard.ChangeState(new MageAttackingState(bossWizard));
                        return;
                    }
                }

                // ============ ДЛЯ ОБЫЧНОГО МАГА ============
                if (_stateMachine is MageStateMachine mage)
                {
                    if (Time.time > mage.LastAttackTime + mage.AttackCooldown)
                    {
                        _isWaitingForAttack = true;
                        mage.ChangeState(new MageAttackingState(mage));
                    }
                    return;
                }

                // ============ ДЛЯ МИЛИШНИКА ============
                if (_stateMachine is MeleeStateMachine melee)
                {
                    if (melee.IsBoss && Time.time > melee.LastStrongAttackTime + melee.StrongAttackCooldown)
                    {
                        _isWaitingForAttack = true;
                        melee.ChangeState(new StrongAttackingState(melee));
                        return;
                    }
                    if (Time.time > melee.LastAttackTime + melee.AttackCooldown)
                    {
                        _isWaitingForAttack = true;
                        melee.ChangeState(new AttackingState(melee));
                        return;
                    }
                }
            }
            return;
        }

        _isWaitingForAttack = false;

        if (dist > _stateMachine.ChaseRange && !_stateMachine.IsAgro)
        {
            _stateMachine.ChangeState(new PatrollingState(_stateMachine));
            return;
        }

        if (_agent.isActiveAndEnabled)
        {
            _agent.isStopped = false;
            _agent.SetDestination(_stateMachine.Player.position);

            if (!_animator.GetBool("IsChasing"))
            {
                _animator.SetBool("IsChasing", true);
            }
        }
    }

    public override void Exit()
    {
        _animator.SetBool("IsChasing", false);
        _isWaitingForAttack = false;
    }
}