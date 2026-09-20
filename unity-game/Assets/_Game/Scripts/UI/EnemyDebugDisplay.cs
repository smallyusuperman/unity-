using UnityEngine;
using TMPro;

// 挂在面板之外的常驻对象上，避免面板隐藏后无法通过开关重新显示。
public class EnemyDebugDisplay : MonoBehaviour
{
    [SerializeField] private EnemyController observedEnemy;

    [SerializeField] private TMP_Text debugText;

    private EnemyController lastObservedEnemy;

    [SerializeField] private bool showPanel = true;
    [SerializeField] private GameObject debugPanel;

    private void Start()
    {
        if (debugPanel == null)
        {
            Debug.LogError("EnemyDebugDisplay: 请绑定 Debug Panel。", this);
            enabled = false;
            return;
        }

        if (debugText == null)
        {
            Debug.LogError("EnemyDebugDisplay: 请绑定 Debug Text。", this);
            enabled = false;
            return;
        }

        debugText.text = "Enemy: Not selected";
    }

    private void Update()
    {
        if (debugPanel.activeSelf != showPanel)
        {
            debugPanel.SetActive(showPanel);
        }

        if (!showPanel)
        {
            return;
        }

        if (observedEnemy == null)
        {
            // 区分"未选中过"与"选过但已被销毁"：Unity 重载的 == 对已销毁对象返回 true
            //（native 对象已不存在，只剩 managed wrapper），而 ReferenceEquals 只看 C# 引用本身。
            // 两者同时成立，说明引用非空但对象已销毁。
            bool previousEnemyDestroyed =
                !object.ReferenceEquals(lastObservedEnemy, null)
                && lastObservedEnemy == null;

            debugText.text = previousEnemyDestroyed
                ? "Enemy: Destroyed"
                : "Enemy: Not selected";

            return;
        }

        lastObservedEnemy = observedEnemy;

        string controllerStatus =
            observedEnemy.isActiveAndEnabled ? "Running" : "Stopped";

        debugText.text =
            $"Enemy: {observedEnemy.name} #{observedEnemy.GetInstanceID()}\n" +
            $"State: {observedEnemy.CurrentState}\n" +
            $"Controller: {controllerStatus}";
    }
}
