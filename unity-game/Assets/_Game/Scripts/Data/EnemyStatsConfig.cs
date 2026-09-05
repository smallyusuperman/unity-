using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyStatsConfig",
    menuName = "Experiments/Enemy Stats Config"
)]
public class EnemyStatsConfig : ScriptableObject
{
    //[Min(0f)] public float moveSpeed;

    [Min(0f)] public float ChaseMovespeed;
    [Min(0f)] public float IdleMovespeed;
    [Min(0f)] public float IdleTime;
    [Min(0f)] public float AttackDamage;
    [Min(0f)] public float AttackCooldown;
    [Min(0f)] public float AttackRange;
    [Min(0f)] public float IdleToChaseDistance;
    [Min(0f)] public float ChaseToIdleDistance;
    [Min(0f)] public float maxHealth;
}