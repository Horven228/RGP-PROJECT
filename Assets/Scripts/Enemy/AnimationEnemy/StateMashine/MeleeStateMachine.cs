using UnityEngine;

public class MeleeStateMachine : BaseEnemyStateMachine
{
    [Header("Босс (Сильная атака)")]
    public bool IsBoss = false;
    public float StrongAttackCooldown = 6f;
    public float StrongAttackDuration = 1.8f;
    public float LastStrongAttackTime { get; set; }

    public override bool IsBossEnemy => IsBoss;
}