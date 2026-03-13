using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage);
    bool IsDead { get; }
    Transform Transform { get; }
}