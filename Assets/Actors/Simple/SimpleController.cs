using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleController : MonoBehaviour {
  public PlayerInput playerInput;
  [Range(0, 10f)] public float speed = 3f;

  private Vector2 move2D = Vector2.zero;
  private Vector3 move = Vector3.zero;

  public void OnMovement(InputAction.CallbackContext context) {
    move2D = context.ReadValue<Vector2>();
    move.x = move2D.x;
    move.z = move2D.y;
  }

  void FixedUpdate() {
    transform.position += move * speed * Time.deltaTime;
  }
}
