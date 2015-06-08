using UnityEngine;
using System.Collections;

public enum DamageType
{
    NORMAL,
    PIERCE,
    SIEGE,
    MAGIC,
    CHAOS
}

public class TowerProperties : MonoBehaviour {
    public DamageType damageType;
    public float damage;
}
