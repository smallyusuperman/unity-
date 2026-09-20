using UnityEngine;

/// <summary>
/// 敌人参数配置资产，被同一类型的多个敌人实例共享。
/// 每个实例的运行期状态（当前血量、冷却、减速倍率等）必须留在组件中，不得写回本资产，
/// 否则会污染所有共用该配置的敌人。
/// </summary>
[CreateAssetMenu(
    fileName = "EnemyStatsConfig",
    menuName = "Experiments/Enemy Stats Config"
)]
public class EnemyStatsConfig : ScriptableObject
{
    [Min(0f)] public float ChaseMovespeed;
    [Min(0f)] public float IdleMovespeed;
    [Min(0f)] public float IdleTime;
    [Min(0f)] public float AttackDamage;
    [Min(0f)] public float AttackCooldown;
    [Min(0f)] public float AttackRange;
    // 两个距离构成迟滞：进入 Chase 用较小的 IdleToChaseDistance，退出 Chase 用较大的 ChaseToIdleDistance，
    // 避免玩家停在阈值边界上时状态反复抖动。合法性约束见 EnemyController.ValidateConfiguration。
    [Min(0f)] public float IdleToChaseDistance;
    [Min(0f)] public float ChaseToIdleDistance;
    [Min(0f)] public float maxHealth;
}