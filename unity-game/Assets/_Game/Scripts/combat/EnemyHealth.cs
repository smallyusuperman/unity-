using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Min(0f)]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        maxHealth = Mathf.Max(0f, maxHealth);
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        damage = Mathf.Max(0f, damage);
        currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
        Debug.Log($"Enemy Health: {currentHealth}/{maxHealth}");
        if (currentHealth == 0f)
        {
            Destroy(gameObject);
            Debug.Log("Enemy destroyed");
        }
    }
}
