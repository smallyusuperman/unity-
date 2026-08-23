using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // 当前场景实例通过 Inspector 指定 Player；敌人会读取该 Transform 的最新位置。
    [SerializeField] private Transform target;

    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (target == null)
        {
            Debug.LogError("EnemyController requires a target Transform.", this);
            enabled = false;
        }
    }

    private void FixedUpdate()
    {
        // 归一化方向，确保追踪速度不随目标距离或斜向分量变化。
        Vector2 direction = (target.position - transform.position).normalized;
        Vector2 newPosition = rb.position + direction * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPosition);
    }
}
