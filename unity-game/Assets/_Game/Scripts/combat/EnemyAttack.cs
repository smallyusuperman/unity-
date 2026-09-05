using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private EnemyStatsConfig enemyStatsConfig;

    private float damage;

    private EnemyState currentState;
    private EnemyController enemyController;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        if (enemyController == null)
        {
            Debug.LogError(
                "EnemyAttack requires an EnemyController component on the same GameObject.",
                this);
            enabled = false;
            return;
        }

        if (enemyStatsConfig != null && enemyStatsConfig.AttackDamage > 0f)
        {
            damage = enemyStatsConfig.AttackDamage;
        }
        else
        {
            Debug.LogWarning(
                "EnemyAttack: No EnemyStatsConfig assigned or AttackDamage is not legal. Using default damage value of 5.",
                this);
            damage = 5f; // 默认伤害值
        }
    }

    private void FixedUpdate()
    {
        currentState = enemyController.CurrentState;
        switch(currentState)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Chase:
                break;
            case EnemyState.Attack:
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, enemyStatsConfig.AttackRange);
                for (int i = 0; i < hitColliders.Length; i++)
                {
                PlayerHealth playerHealth = hitColliders[i].GetComponent<PlayerHealth>();
                if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(damage);
                    }
                }

                enemyController.ResetCooldown();

                enemyController.ChangeState(EnemyState.Chase);
                break;
            default:
                Debug.LogWarning(
                    $"EnemyAttack: Unhandled EnemyState {currentState}.",
                    this);
                break;
        }
    }
}
