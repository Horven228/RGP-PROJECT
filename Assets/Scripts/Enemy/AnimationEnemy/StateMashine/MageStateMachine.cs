using UnityEngine;

public class MageStateMachine : BaseEnemyStateMachine
{
    [Header("Магия")]
    public GameObject ProjectilePrefab;
    public Transform ShootPoint;

    [Header("Настройки босса")]
    public bool IsBoss = false;

    public MageAttackingState MageAttackState { get; private set; }

    // Переопределяем свойство для босса
    public override bool IsBossEnemy => IsBoss;

    protected override void Awake()
    {
        AttackRange = 10f;
        ChaseRange = 18f;

        base.Awake();
        MageAttackState = new MageAttackingState(this);
    }

    private void Start() => ChangeState(PatrolState);

    public void SpawnMagicBall()
    {
        if (ProjectilePrefab != null && ShootPoint != null)
        {
            Instantiate(ProjectilePrefab, ShootPoint.position, ShootPoint.rotation);
        }
    }
}