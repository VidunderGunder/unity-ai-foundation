using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleController : MonoBehaviour
{
    private Vector3 move = Vector3.zero;

    private Vector2 move2D = Vector2.zero;
    public PlayerInput playerInput;
    [Range(0, 10f)] public float speed = 3f;

    public void OnMovement(InputAction.CallbackContext context)
    {
        move2D = context.ReadValue<Vector2>();
        move.x = move2D.x;
        move.z = move2D.y;
    }

    private void FixedUpdate()
    {
        transform.position += move * speed * Time.deltaTime;
    }
}