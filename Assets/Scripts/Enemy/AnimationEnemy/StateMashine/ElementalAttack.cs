using System;
using UnityEngine;

[Serializable]
public class ElementalAttack
{
    public ElementType elementType;
    public GameObject normalProjectile;  // Обычный снаряд
    public GameObject strongProjectile;  // Сильный снаряд

    [Header("Эффекты при столкновении (привязаны к оружию)")]
    public GameObject hitEffect_StaffPhase1;  // Эффект для StaffPhase1
    public GameObject hitEffect_StaffPhase2;  // Эффект для StaffPhase2
}