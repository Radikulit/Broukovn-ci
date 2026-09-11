using System.Drawing;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public GameObject unitPrefab;
    public int health;
    public int armor;
    public int damage;
    public int speed;
    public int range;
    public int psycho;
}
