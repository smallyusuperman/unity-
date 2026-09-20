using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;

    private Vector2 movement;

    [Min(0.01f)]
    [SerializeField] private float moveSpeed = 5f;

    private void Awake()
    {
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
        Vector2 newposition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newposition);
    }
}
