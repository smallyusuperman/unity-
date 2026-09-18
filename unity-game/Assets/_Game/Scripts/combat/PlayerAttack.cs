using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerAttack : MonoBehaviour
{
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
        attackInterval = Mathf.Max(0.01f, normalAttackConfig.AttackInterval);
    }

    private void Start()
    {
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
