using UnityEngine;

public class BossWizardStrongAttackingState : EnemyState
{
    private float _timer;
    private BossWizardStateMachine _bossMachine;
    private bool _hasPerformedAttack;

    public BossWizardStrongAttackingState(BossWizardStateMachine stateMachine) : base(stateMachine)
    {
        _bossMachine = stateMachine;
    }

    public override void Enter()
    {
        if (_agent.isActiveAndEnabled) _agent.isStopped = true;
        _agent.velocity = Vector3.zero;

        ResetAllBools();
        _animator.SetBool("IsStrongAttacking", true);

        _timer = _stateMachine.AttackDuration * 1.2f;
        _hasPerformedAttack = false;

        // Задержка перед выполнением сильной атаки (половина длительности анимации)
        float delay = _stateMachine.AttackDuration * 0.5f;
        _bossMachine.Invoke(nameof(PerformStrongAttack), delay);

        Debug.Log($"[BossWizardStrongAttackingState] Entered. Attack will be performed after {delay} seconds");
    }

    private void PerformStrongAttack()
    {
        if (_hasPerformedAttack) return;
        _hasPerformedAttack = true;
        _bossMachine.PerformStrongAttack();
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
        _bossMachine.CancelInvoke(nameof(PerformStrongAttack));

        Debug.Log("[BossWizardStrongAttackingState] Exited");
    }
}