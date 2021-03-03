using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleController : MonoBehaviour {
  public PlayerInput playerInput;
  public List<WheelCollider> wheelColliders;
  public List<Transform> wheels;
  [Range(0, 1000f)] public float maxTorque = 100f;
  [Range(0, 90f)] public float maxSteerAngle = 30f;

  private float torque;
  private float steerAngle;

  void ApplyTorque(float input) {
    input = Mathf.Clamp(input, -1f, 1f);
    torque = input * maxTorque;

    foreach (var wheel in wheelColliders) {
      wheel.motorTorque = torque;
    }
  }

  void ApplySteer(float input) {
    input = Mathf.Clamp(input, -1f, 1f);
    steerAngle = input * maxSteerAngle;
    Debug.Log("Apply Steer: " + steerAngle.ToString());

    for (int wheelIndex = 0; wheelIndex < 2; wheelIndex++) {
      WheelCollider wheelCollider = wheelColliders[wheelIndex];
      wheelCollider.steerAngle = steerAngle;
    }
  }

  public void OnMovement(InputAction.CallbackContext context) {
    Vector2 movement = context.ReadValue<Vector2>();
    ApplySteer(movement.x);
    ApplyTorque(movement.y);
  }

  void Update() {
    for (int wheelIndex = 0; wheelIndex < wheelColliders.Count; wheelIndex++) {
      WheelCollider wheelCollider = wheelColliders[wheelIndex];
      Transform wheel = wheels[wheelIndex];

      Vector3 wheelPosition;
      Quaternion wheelRotation;
      wheelCollider.GetWorldPose(out wheelPosition, out wheelRotation);
      wheel.position = wheelPosition;
      wheel.rotation = wheelRotation;
    }
  }
}
