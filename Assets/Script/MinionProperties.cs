using UnityEngine;
using System.Collections;

public enum ArmorType
{
    UNARMED,
    LIGHT,
    MEDIUM,
    HEAVY,
    FORTIFIED
}

public class MinionProperties : MonoBehaviour {
    public float hp;
    public ArmorType armorType;
    public float armor;
    public float speed;
    public int pathIndex;
}
