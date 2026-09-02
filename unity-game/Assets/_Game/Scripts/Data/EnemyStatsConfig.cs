using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyStatsConfig",
    menuName = "Experiments/Enemy Stats Config"
)]
public class EnemyStatsConfig : ScriptableObject
{
    [Min(0f)] public float moveSpeed;
    public float maxHealth;
}