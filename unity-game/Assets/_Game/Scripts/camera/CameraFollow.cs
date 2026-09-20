using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    private void Awake()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow requires a target Transform.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        // 用 LateUpdate 而非 Update：等本帧所有 Update 逻辑（含玩家移动）执行完再取位置，
        // 否则摄像机取到的是上一帧的坐标，画面会抖动。保留原 z 值，2D 相机不参与前后位移。
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }
}
