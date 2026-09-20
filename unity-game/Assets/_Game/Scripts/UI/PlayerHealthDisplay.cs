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
        // 主动刷一次初值：订阅发生在 OnEnable，早于 Start 的事件（含 PlayerHealth.Awake 里那次通知）收不到。
        // 靠这次调用把显示拉到当前状态，从而不依赖 Awake 的执行顺序。
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
        // 与 OnDisable 成对订阅 / 退订：组件停用期间不再响应事件，重开后也不会重复订阅。
        playerHealth.HealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        // 判空用于应对 PlayerHealth 先于本组件被销毁的情况，避免对失效引用继续操作。
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= OnHealthChanged;
        }
    }
}
