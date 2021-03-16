using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CinemachineSwitch : MonoBehaviour {
  [SerializeField]
  private InputAction action;
  //   private List<CinemachineVirtualCamera> virtualCameras;
  [SerializeField]
  private CinemachineVirtualCamera vcam1;
  [SerializeField]
  private CinemachineVirtualCamera vcam2;

  private Animator animator;
  private bool overworldCamera = true;

  private void Awake() {
    // animator = GetComponent<Animator>();
  }

  private void OnEnable() {
    action.Enable();
  }

  private void OnDisable() {
    action.Disable();
  }

  private void Start() {
    // action.performed += ctx => SwitchState();
    action.performed += ctx => SwitchPriority();
  }

  private void SwitchState() {
    if (overworldCamera) {
      animator.Play(0);
    } else {
      animator.Play(1);
    }
    overworldCamera = !overworldCamera;
  }

  private void SwitchPriority() {
    if (overworldCamera) {
      vcam1.Priority = 0;
      vcam2.Priority = 1;
    } else {
      vcam1.Priority = 1;
      vcam2.Priority = 0;
    }
    overworldCamera = !overworldCamera;
  }
}
