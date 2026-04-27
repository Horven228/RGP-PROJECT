using UnityEngine;

public class MeleeStateMachine : BaseEnemyStateMachine
{
    [Header("Босс (Сильная атака)")]
    public bool IsBoss = false;
    public float StrongAttackCooldown = 6f;
    public float StrongAttackDuration = 1.8f;
    public float LastStrongAttackTime { get; set; }

    public AttackingState AttackState { get; private set; }
    public StrongAttackingState StrongAttackState { get; private set; }

    // Переопределяем свойство для босса
    public override bool IsBossEnemy => IsBoss;

    protected override void Awake()
    {
        base.Awake();
        AttackState = new AttackingState(this);
        StrongAttackState = new StrongAttackingState(this);
    }

    private void Start() => ChangeState(PatrolState);
}