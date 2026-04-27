using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public int health;
    public float magicCooldownRemaining; 
    public bool isMagicOnCooldown;

}

[Serializable]
public class EnemySaveData
{
    public string id;
    public float[] position;
    public int health;
    public bool isDead;
}

[Serializable]
public class GameSaveData
{
    public PlayerSaveData player = new PlayerSaveData();
    public List<EnemySaveData> enemies = new List<EnemySaveData>();
}