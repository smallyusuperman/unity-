using UnityEngine;

/// <summary>
/// 玩家技能的共享配置资产（近战与投射物各用一份）。
/// 只保存设计期参数；每个施法者的冷却等运行期状态留在组件里，不写回本资产。
/// </summary>
[CreateAssetMenu(fileName = "PlayerSkillConfig", menuName = "Game/Skills/Player Skill Config")]
public class PlayerSkillConfig : ScriptableObject
{
    [Min(0f)]
    [SerializeField] private float attackDamage = 30f;

    [Min(0f)]
    [SerializeField] private float attackInterval = 1f;

    [Min(0f)]
    [SerializeField] private float attackRange = 2f;

    /// <summary>单次伤害；读取时钳制为非负，字段本身保留 Inspector 中填写的原值。</summary>
    public float AttackDamage => Mathf.Max(0f, attackDamage);
    /// <summary>两次释放之间的最小间隔（秒）。</summary>
    public float AttackInterval => Mathf.Max(0f, attackInterval);
    /// <summary>作用范围；近战中作为命中半径，投射物中作为索敌半径。</summary>
    public float AttackRange => Mathf.Max(0f, attackRange);
}
