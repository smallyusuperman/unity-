using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // 场景实例可由 Inspector 配置目标；动态实例由 WaveSpawner 调用 Initialize 注入目标。
    [SerializeField] private Transform target;

    [Min(0f)][SerializeField] private float moveSpeed = 2f;

    [SerializeField] private EnemyStatsConfig enemyData;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (target == null)
        {
            enabled = false;
        }
        if(enemyData != null)
        {
           moveSpeed = enemyData.moveSpeed;
        }
        
    }

    public void Initialize(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogError(
                "EnemyController cannot initialize without a target Transform.",
                this);

            enabled = false;
            return;
        }

        target = newTarget;
        enabled = true;
    }

    private void FixedUpdate()
    {
        // 归一化方向，确保追踪速度不随目标距离或斜向分量变化。
        Vector2 direction = (target.position - transform.position).normalized;
        Vector2 newPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }
}
