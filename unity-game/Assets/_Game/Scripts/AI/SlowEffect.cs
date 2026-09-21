using UnityEngine;

public class SlowEffect : MonoBehaviour
{
    private float speedprecentage = 1f;
    private float slowDuration;

    public float SpeedMultiper => speedprecentage;

    public event System.Action Expired;

    public void SetSlowEffect(float precentage, float duration)
    {
        if (isActiveAndEnabled && precentage > 0 && precentage <=1 && duration > 0 && !float.IsInfinity(duration))
        {
            slowDuration = duration;
            speedprecentage = precentage;
        }
        else
        {
            Debug.LogError("SlowEffect: Invalid speedprecentage or duration");
        }
    }

    private void FixedUpdate()
    {
        if (slowDuration <= 0f) return;
        slowDuration = Mathf.Max(0f, slowDuration - Time.fixedDeltaTime);
        if (slowDuration <= 0)
        {
            speedprecentage = 1;
            // 先恢复状态，再通知，允许订阅者在回调中重新施加效果。
            Expired?.Invoke();
        }
    }

    private void OnDisable()
    {
        slowDuration = 0f;
        speedprecentage = 1f;
    }
}
