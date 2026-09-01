using UnityEngine;
using TMPro;
using System;

public class PlayerHealthDisplay : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;

    private TMP_Text healthText;

    private void Awake(){
        if (playerHealth == null){
            Debug.LogError("PlayerHealthDisplay requires a PlayerHealth reference.",this);
            enabled = false;
            return;
        }

        healthText = GetComponent<TMP_Text>();
        if (healthText == null){
            Debug.LogError("PlayerHealthDisplay requires a TMP_Text component.",this);
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        // 初始化显示
        OnHealthChanged(playerHealth, EventArgs.Empty);
    }

    private void OnHealthChanged(object sender, EventArgs e)
    {
        healthText.text = $"Health: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}";

        if (playerHealth.CurrentHealth <= 0f)
        {
            healthText.text = "!you are dead! Press R to Restart";
        }
    }

    private void OnEnable()
    {
        playerHealth.HealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= OnHealthChanged;
        }
    }
}
