using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerAttack : MonoBehaviour
{
    [FormerlySerializedAs("playerDamage")]
    [Min(0f)]
    [SerializeField] private float attackDamage = 10f;

    [FormerlySerializedAs("striking_distance")]
    [Min(0f)]
    [SerializeField] private float attackRange = 1f;

    private void Awake()
    {
        attackDamage = Mathf.Max(0f, attackDamage);
        attackRange = Mathf.Max(0f, attackRange);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
        {
            Attack();
        }
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
