using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerAttack : MonoBehaviour
{
    // 该字段曾用名 NormalAttack；保留此属性后，旧 Prefab / Scene 上已保存的引用不会断链。
    [FormerlySerializedAs("NormalAttack")]
    [SerializeField] private PlayerSkillConfig normalAttackConfig;

    private float attackDamage;
    private float attackRange;
    private float attackInterval;
    private float remainingCooldown;

    private void Awake()
    {
        if (normalAttackConfig == null)
        {
            Debug.LogError("PlayerAttack requires a PlayerSkillConfig.", this);
            enabled = false;
            return;
        }

        attackDamage = Mathf.Max(0f, normalAttackConfig.AttackDamage);
        attackRange = Mathf.Max(0f, normalAttackConfig.AttackRange);
        // 间隔下限取 0.01 秒：配置为 0 会让每次 Update 都立即命中，等于没有冷却。
        attackInterval = Mathf.Max(0.01f, normalAttackConfig.AttackInterval);
    }

    private void Start()
    {
        // 开局即进入一轮冷却，使第一次攻击也要等满一个间隔。
        remainingCooldown = attackInterval;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame && remainingCooldown <= 0f)
        {
            Attack();
            remainingCooldown = attackInterval;
        }
    }

    private void FixedUpdate()
    {
        // 冷却在固定步长中推进；输入判定留在 Update，因为 wasPressedThisFrame 按渲染帧计数，
        // 放进 FixedUpdate 会丢按键。
        remainingCooldown -= Time.fixedDeltaTime;
    }

    // 当前 Enemy prefab 只有一个 Collider；若未来添加多个 Collider，需要按 EnemyHealth 去重。
    private void Attack()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            EnemyHealth enemyHealth = hitColliders[i].GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
