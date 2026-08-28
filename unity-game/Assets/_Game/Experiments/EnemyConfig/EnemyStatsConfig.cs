using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyStatsConfig",
    menuName = "Experiments/Enemy Stats Config"
)]
public class EnemyStatsConfig : ScriptableObject
{
    public float moveSpeed;
    public float maxHealth;
}