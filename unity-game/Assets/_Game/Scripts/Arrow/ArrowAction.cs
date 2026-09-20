using UnityEngine;

public class ArrowAction : MonoBehaviour
{
    private float speed;
    private float damage;
    private float maxDistance;
    private Vector3 direction;
    private Vector3 startPosition;

    public void Initialize(Vector3 direction, float speed, float damage, float maxDistance)
    {
        if(direction != Vector3.zero && direction.z == 0f && speed > 0f && damage > 0f && maxDistance > 0f){
            this.direction = direction.normalized;
            this.speed = speed;
            this.damage = damage;
            this.maxDistance = maxDistance;
            startPosition = transform.position;
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
        transform.position += direction * speed * Time.deltaTime;
        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
       EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            // 允许同一物理步内收到的多个敌人接触回调分别结算伤害。
            enemyHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if(other.GetComponent<PlayerHealth>() == null && other.GetComponent<ArrowAction>() == null)
        {
            Destroy(gameObject);
        }
}
}
