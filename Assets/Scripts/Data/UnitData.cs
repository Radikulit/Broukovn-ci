using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public GameObject unitPrefab;
    public float health;
    public float armor;
    public float damage;
    public float speed;
    public float range;
    public float psycho;
    public int unitSize;
}
