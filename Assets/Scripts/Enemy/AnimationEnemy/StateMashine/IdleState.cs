using UnityEngine;

public class IdleState : EnemyState
{
    private float _idleTimer;
    private float _idleDuration = 5f;

    public IdleState(BaseEnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        ResetAllBools();

        // Останавливаем движение
        if (_agent.isActiveAndEnabled)
        {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
        }

        // Включаем анимацию Idle (если есть)
        if (HasParameter("IsIdle"))
            _animator.SetBool("IsIdle", true);

        _idleTimer = _idleDuration;

    }

    public override void Update()
    {
        _idleTimer -= Time.deltaTime;

        // Проверяем, не появился ли игрок рядом во время ожидания
        if (_stateMachine.Player != null)
        {
            float dist = Vector3.Distance(_transform.position, _stateMachine.Player.position);

            bool isPeaceful = GameModeManager.Instance != null &&
                              GameModeManager.Instance.CurrentMode == GameMode.Peaceful;

            if (!isPeaceful && dist < _stateMachine.ChaseRange)
            {
                _stateMachine.ChangeState(new ChasingState(_stateMachine));
                return;
            }
        }

        // Время ожидания закончилось - продолжаем патрулирование к следующей точке
        if (_idleTimer <= 0)
        {
            _stateMachine.ChangeState(new PatrollingState(_stateMachine));
        }
    }

    public override void Exit()
    {
        // Выключаем анимацию Idle
        if (HasParameter("IsIdle"))
            _animator.SetBool("IsIdle", false);

        // Возобновляем движение
        if (_agent.isActiveAndEnabled)
        {
            _agent.isStopped = false;
        }

        Debug.Log($"[IdleState] Exit");
    }

    private bool HasParameter(string paramName)
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