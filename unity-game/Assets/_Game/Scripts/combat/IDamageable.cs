/// <summary>
/// 受伤契约：只约定"谁能被打"，不负责敌我筛选。
/// 由 PlayerHealth 与 EnemyHealth 实现；由调用方（PlayerAttack / PlayerShoot / EnemyAttack）
/// 自行决定作用范围与目标选取规则。
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 施加一次伤害。实现方应把负值归零，并自行处理死亡流程。
    /// </summary>
    /// <param name="damage">伤害值，期望为非负。</param>
    void TakeDamage(float damage);
}
