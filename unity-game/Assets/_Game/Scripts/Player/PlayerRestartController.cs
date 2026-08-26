using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerRestartController : MonoBehaviour
{
    private PlayerHealth playerHealth;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("PlayerRestartController requires a PlayerHealth component.", this);
            enabled = false;
        }
    }

    void Update()
    {
        if (playerHealth.CurrentHealth <= 0f)
        {

            if (Keyboard.current == null){
                Debug.LogError("Keyboard input is not available.");
                return;
            }
            // 按下 R 键时重新加载当前场景。
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                Debug.Log("Player health reset.");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
