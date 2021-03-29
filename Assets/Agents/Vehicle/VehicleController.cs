using System.Collections.Generic;
using Unity.MLAgents.Extensions.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleController : MonoBehaviour, IInputActionAssetProvider
{
    protected PlayerActions agentActions;
    public Rigidbody body;

    [Range(0, 2f)] private bool boost;
    public bool boostAlwaysOn = false;
    [Range(0, 5f)] public float boostModifier = 2.5f;
    [Range(0, 10000f)] public float brakeTorque = 10000f;
    public Transform centerOfMass;
    [Range(0, 100000f)] public float handbrakeTorque = 50000f;
    private bool handbrake;
    public bool manualControl = false;
    [Range(0, 1000f)] public float maxRpm = 250f;
    [Range(0, 90f)] public float maxSteerAngle = 30f;
    [Range(0, 2000f)] public float maxTorque = 2500f;
    protected PlayerActions playerActions;
    public PlayerInput playerInput;
    private float steerAngle;
    private float torque;
    public List<WheelCollider> wheelColliders;
    public List<Transform> wheels;

    // Required for Input Actuator (autamatic machine learning output)
    public (InputActionAsset, IInputActionCollection2) GetInputActionAsset()
    {
        LazyInitializeActions(ref agentActions);
        return (agentActions.asset, agentActions);
    }

    protected void LazyInitializeActions(ref PlayerActions actions)
    {
        if (actions != null) return;

        actions = new PlayerActions();
        actions.Enable();

        // TODO: Combine started/performed/canceled somehow
        // This is too verbose
        actions.ActorVehicle.Movement.started += OnMovement;
        actions.ActorVehicle.Movement.performed += OnMovement;
        actions.ActorVehicle.Movement.canceled += OnMovement;
        // actions.ActorVehicle.Boost.started += OnBoost;
        // actions.ActorVehicle.Boost.performed += OnBoost;
        // actions.ActorVehicle.Boost.canceled += OnBoost;
        actions.ActorVehicle.Handbrake.started += OnHandbrake;
        actions.ActorVehicle.Handbrake.performed += OnHandbrake;
        actions.ActorVehicle.Handbrake.canceled += OnHandbrake;
    }

    private void OnAwake()
    {
        // https://docs.unity3d.com/ScriptReference/WheelCollider.ConfigureVehicleSubsteps.html
        var wheelColliders = GetComponentInChildren<WheelCollider>();
        wheelColliders.ConfigureVehicleSubsteps(2f, 12, 16);
        body.centerOfMass = centerOfMass.position;
    }

    private void Start()
    {
        if (manualControl) LazyInitializeActions(ref playerActions);
    }

    private void SetTorque(float input)
    {
        input = Mathf.Clamp(input, -1f, 1f);
        torque = input * maxTorque * (boost | boostAlwaysOn ? boostModifier : 1f);
    }

    private void SetSteer(float input)
    {
        input = Mathf.Clamp(input, -1f, 1f);
        steerAngle = input * maxSteerAngle;
    }

    private void ApplyTorque()
    {
        var wheelIndex = 0;
        foreach (var wheel in wheelColliders)
        {
            if (Mathf.Abs(wheel.rpm) < (boost | boostAlwaysOn ? maxRpm * 3 : maxRpm))
                wheel.motorTorque = torque;
            else
                wheel.motorTorque = 0;
            ;

            if (handbrake)
            {
                wheel.brakeTorque = handbrakeTorque;
                // if (wheelIndex >= 2) wheel.brakeTorque = handbrakeTorque;
                wheel.motorTorque = 0;
            }
            else
            {
                if (((wheel.rpm > 0) & (torque < 0)) | ((wheel.rpm < 0) & (torque > 0)))
                    wheel.brakeTorque = brakeTorque;
                else
                    wheel.brakeTorque = 0;
            }

            wheelIndex++;
        }
    }

    private void ApplySteer()
    {
        for (var wheelIndex = 0; wheelIndex < 2; wheelIndex++)
        {
            var wheelCollider = wheelColliders[wheelIndex];
            wheelCollider.steerAngle = steerAngle;
        }
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        var movement = context.ReadValue<Vector2>();
        SetSteer(movement.x);
        SetTorque(movement.y);
    }

    public void OnBoost(InputAction.CallbackContext context)
    {
        if (context.started) boost = true;
        if (context.canceled) boost = false;
    }

    public void OnHandbrake(InputAction.CallbackContext context)
    {
        if (context.started) handbrake = true;
        if (context.canceled) handbrake = false;
    }

    public void SyncVisualWheels()
    {
        for (var wheelIndex = 0; wheelIndex < wheelColliders.Count; wheelIndex++)
        {
            var wheelCollider = wheelColliders[wheelIndex];
            var wheel = wheels[wheelIndex];

            Vector3 wheelPosition;
            Quaternion wheelRotation;
            wheelCollider.GetWorldPose(out wheelPosition, out wheelRotation);
            wheel.position = wheelPosition;
            wheel.rotation = wheelRotation;
        }
    }

    private void OnValidate()
    {
        SyncVisualWheels();
    }

    private void FixedUpdate()
    {
        ApplyTorque();
        ApplySteer();
    }

    private void Update()
    {
        SyncVisualWheels();
    }
}