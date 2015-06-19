using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public float rotationSpeed;
    public List<LevelLoader.TilePoint> walkPath;
    public int walkPathIndex;
    public int pathIndex;
    public LevelLoader.TilePoint currentTile;
}
