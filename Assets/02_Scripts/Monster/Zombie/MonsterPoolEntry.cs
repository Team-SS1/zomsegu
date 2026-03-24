using MonsterEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MonsterPoolEntry
{
    public ZombieType type;
    public Zombie prefab;
    public int initialSize = 10;
}