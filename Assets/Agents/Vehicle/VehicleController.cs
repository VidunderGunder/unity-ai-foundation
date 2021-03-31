using System.Collections.Generic;
using Unity.MLAgents.Extensions.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleController : MonoBehaviour, IInputActionAssetProvider
{
    public EnvironmentData env;
    public Rigidbody body;
    public Transform centerOfMass;
    public PlayerInput playerInput;
    public List<WheelCollider> wheelColliders;
    public List<Transform> wheels;

    [Range(0, 100000f)] public float brakeTorque = 20000f;
    [Range(0, 100000f)] public float handbrakeTorque = 50000f;
    [Range(0, 10000f)] public float maxRpm = 250f;
    [Range(0, 90f)] public float maxSteerAngle = 30f;
    [Range(0, 2000f)] public float maxTorque = 2500f;
    [Tooltip("Meters/second")]
    [Range(0, 50f)] public float maxSpeed = 20f;
    [Range(0, 50f)] public float maxSpeedReverse = 10f;
    [Range(0, 2f)] public float speedDeadzone = 1f;
    [Range(0, 0.1f)] public float inputDeadzone = 0.05f;
    [Range(0, 5f)] public float boostModifier = 2.5f;

    public bool boostAlwaysOn = false;
    public bool manualControl = false;

    protected PlayerActions playerActions;
    protected PlayerActions agentActions;

    private bool boost;
    private float torque;
    private bool handbrake;
    private float steerAngle;
    private float wantedSpeed;

    public float Speed => body.transform.InverseTransformDirection(body.velocity).z;

    // Required for Input Actuator (automatic machine learning output)
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
        // actions.ActorVehicle.Handbrake.started += OnHandbrake;
        // actions.ActorVehicle.Handbrake.performed += OnHandbrake;
        // actions.ActorVehicle.Handbrake.canceled += OnHandbrake;
    }

    private void OnAwake()
    {
        // https://docs.unity3d.com/ScriptReference/WheelCollider.ConfigureVehicleSubsteps.html
        foreach (var collider in wheelColliders)
        {
            // collider.ConfigureVehicleSubsteps(2f, 16, 64);
        }
        body.centerOfMass = centerOfMass.position;
    }

    private void Start()
    {
        if (env != null && env.isTraining) manualControl = false;
        if (manualControl)
        {
            LazyInitializeActions(ref playerActions);
        }
    }

    private void SetWantedSpeed(float input)
    {
        input = Mathf.Clamp(input, -1f, 1f);
        wantedSpeed = input > 0 ? input * maxSpeed : input * maxSpeedReverse;

        if (Speed < wantedSpeed - maxSpeed * 0.01f)
        {
            SetTorque(maxTorque);
        }
        else if (Speed > wantedSpeed + maxSpeed * 0.01f)
        {
            SetTorque(-maxTorque);
        }
        else
        {
            SetTorque(0);
        }
    }

    private void SetTorque(float input)
    {
        input = Mathf.Clamp(input, -1f, 1f);
        if (Mathf.Abs(input) < inputDeadzone)
        {
            torque = 0;
        }
        else
        {
            torque = input * maxTorque * (boost | boostAlwaysOn ? boostModifier : 1f);
        }
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
            {
                wheel.motorTorque = torque;
            }
            else
            {
                wheel.motorTorque = 0;
            }

            if (handbrake)
            {
                wheel.brakeTorque = handbrakeTorque;
                wheel.motorTorque = 0;
            }
            else
            {
                if (((wheel.rpm > 0) & (torque < 0)) | ((wheel.rpm < 0) & (torque > 0)))
                {
                    wheel.brakeTorque = brakeTorque;
                }
                else
                {
                    wheel.brakeTorque = 0;
                }
            }

            if (Mathf.Abs(Speed) < speedDeadzone && Mathf.Abs(torque) < 0.001f)
            {
                wheel.motorTorque = 0;
                wheel.brakeTorque = brakeTorque;
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
        if (manualControl)
        {
            SetTorque(movement.y);
        }
        else
        {
            SetWantedSpeed(movement.y);
        }
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
        // Debug.Log(
        //     Speed.ToString("F1")
        //     + "m/s -> "
        //     + wantedSpeed.ToString("F1") + "m/s"
        // );
    }

    public void ResetController()
    {
        SetSteer(0);
        SetWantedSpeed(0);
        SetTorque(0);

        foreach (var wheel in wheelColliders)
        {
            wheel.brakeTorque = Mathf.Infinity;
        }

        ApplyTorque();
        ApplySteer();
    }
}