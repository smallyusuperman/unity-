using UnityEngine;

public class SlowArrowAction : MonoBehaviour
{
    private float speed;
    private float damage;
    private float maxDistance;
    private Vector3 direction;
    private Vector3 startPosition;
    private float speedMultiper;
    private float slowDuration;

    /// <summary>
    /// 由发射方在 Instantiate 之后立即调用，注入本次飞行的参数。
    /// 参数非法时记录错误并销毁自身，避免留下一个不会移动的投射物。
    /// </summary>
    /// <param name="direction">飞行方向；必须非零且 z 分量为 0（本游戏为 2D 平面）。</param>
    /// <param name="speed">飞行速度，必须大于 0。</param>
    /// <param name="damage">命中伤害，必须大于 0。</param>
    /// <param name="maxDistance">最大飞行距离，必须大于 0；超出后自动销毁。</param>
    public void Initialize(Vector3 direction, float speed, float damage, float maxDistance, float speedMultiper, float slowDuration)
    {
        if (direction != Vector3.zero && direction.z == 0f && speed > 0f && damage > 0f && maxDistance > 0f && speedMultiper > 0f && speedMultiper <= 1f && slowDuration > 0f)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            this.damage = damage;
            this.maxDistance = maxDistance;
            this.speedMultiper = speedMultiper;
            this.slowDuration = slowDuration;
            startPosition = transform.position;
            // 用 transform.right 对齐朝向。2D 下 direction 的 z 分量会让精灵绕 z 轴偏转，
            // 这正是上面要求 direction.z == 0 的原因。
            transform.right = direction;
        }
        else
        {
            Debug.LogError("Invalid parameters for ArrowAction initialization.");
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        // 位移必须使用 fixedDeltaTime，与 FixedUpdate 的调用间隔保持一致。
        // 若误用 Time.deltaTime，飞行速度会随渲染帧率变化（高帧率下明显变慢）。
        transform.position += direction * speed * Time.fixedDeltaTime;
        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        SlowEffect slowEffect = other.GetComponent<SlowEffect>();
        if (enemyHealth != null && slowEffect != null)
        {
            // 允许同一物理步内收到的多个敌人接触回调分别结算伤害。
            enemyHealth.TakeDamage(damage);
            if (enemyHealth.CurrentHealth > 0f)
            {
                slowEffect.SetSlowEffect(speedMultiper, slowDuration);
            }
            Destroy(gameObject);
        }
        else if (other.GetComponent<PlayerHealth>() == null && other.GetComponent<ArrowAction>() == null && other.GetComponent<SlowArrowAction>() == null)
        {
            // 命中玩家或其它投射物时穿透，其余碰撞体（墙、障碍）一律销毁。
            Destroy(gameObject);
        }
    }
}
