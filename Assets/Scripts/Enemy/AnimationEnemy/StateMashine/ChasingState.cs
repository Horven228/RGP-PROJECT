using UnityEngine;

public class ChasingState : EnemyState
{
    private bool _isWaitingForAttack = false;

    public ChasingState(BaseEnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        ResetAllBools();
        _isWaitingForAttack = false;

        // Проверяем дистанцию ДО включения анимации бега
        if (_stateMachine.Player != null)
        {
            float dist = Vector3.Distance(_transform.position, _stateMachine.Player.position);

            // Если игрок в радиусе атаки - не включаем бег
            if (dist <= _stateMachine.AttackRange)
            {
                if (_agent.isActiveAndEnabled) _agent.isStopped = true;
                _animator.SetBool("IsChasing", false);
                _stateMachine.LookAtPlayer();
                return; // Выходим, не включая бег
            }
        }

        // Включаем анимацию бега только если игрок далеко
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

        // Если в радиусе атаки
        if (dist <= _stateMachine.AttackRange)
        {
            // Останавливаем движение
            if (_agent.isActiveAndEnabled) _agent.isStopped = true;

            // Выключаем анимацию бега
            if (_animator.GetBool("IsChasing"))
            {
                _animator.SetBool("IsChasing", false);
            }

            // Поворачиваемся к игроку
            _stateMachine.LookAtPlayer();

            // Проверяем возможность атаки
            if (!_isWaitingForAttack)
            {
                if (_stateMachine is MageStateMachine mage)
                {
                    if (Time.time > mage.LastAttackTime + mage.AttackCooldown)
                    {
                        _isWaitingForAttack = true;
                        mage.ChangeState(mage.MageAttackState);
                    }
                    return;
                }

                if (_stateMachine is MeleeStateMachine melee)
                {
                    if (melee.IsBoss && Time.time > melee.LastStrongAttackTime + melee.StrongAttackCooldown)
                    {
                        _isWaitingForAttack = true;
                        melee.ChangeState(melee.StrongAttackState);
                        return;
                    }
                    if (Time.time > melee.LastAttackTime + melee.AttackCooldown)
                    {
                        _isWaitingForAttack = true;
                        melee.ChangeState(melee.AttackState);
                        return;
                    }
                }
            }
            return;
        }

        // Сбрасываем флаг ожидания, если вышли из радиуса атаки
        _isWaitingForAttack = false;

        // Если игрок далеко И нет режима агрессии - возвращаемся к патрулю
        if (dist > _stateMachine.ChaseRange && !_stateMachine.IsAgro)
        {
            _stateMachine.ChangeState(_stateMachine.PatrolState);
            return;
        }

        // Продолжаем погоню
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