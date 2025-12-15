using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "SO/Enemy/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    //public Sprite Icon;
    //public Color TintColor = Color.white;

    public int MaxHp = 5;
    public float MoveSpeed = 5f;
    public int Damage = 1;
}