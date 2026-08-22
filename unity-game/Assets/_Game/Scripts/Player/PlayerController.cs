using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // 1. 保存 Rigidbody2D
    private Rigidbody2D rb;

    // 2. 保存玩家当前想移动的方向
    private Vector2 movement;

    // 3. 移动速度，希望以后能在 Inspector 修改
    [Min(0.01f)]
    [SerializeField] private float moveSpeed = 5f;

    private void Awake()
    {
        // 4. 找到挂在当前 Player 上的 Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
{
    movement = Vector2.zero;

    if (Keyboard.current.wKey.isPressed)
    {
        movement.y += 1;
    }

    if (Keyboard.current.sKey.isPressed)
    {
        movement.y -= 1;
    }

    if (Keyboard.current.aKey.isPressed)
    {
        movement.x -= 1;
    }

    if (Keyboard.current.dKey.isPressed)
    {
        movement.x += 1;
    }

    movement = movement.normalized;
}

    private void FixedUpdate()
    {
        // 6. 这里以后负责真正移动 Rigidbody2D
        Vector2 newposition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newposition);
    }
}