using UnityEngine;

public class ContactDamage : MonoBehaviour
{
    [SerializeField] private float damage = 5f;

    // 只在建立新接触时造成一次伤害；持续接触不会按物理步重复扣血。
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("发生碰撞了");

        PlayerHealth playerHealth =
            collision.gameObject.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Debug.Log("造成伤害");
        }
    }
}
