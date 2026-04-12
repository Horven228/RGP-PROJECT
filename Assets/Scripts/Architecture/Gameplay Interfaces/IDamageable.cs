using UnityEngine;

// Описывает методы для получения урона
public interface IDamageable
{
    void TakeDamage(int damage);
    bool IsDead { get; }
    Transform Transform { get; }
}