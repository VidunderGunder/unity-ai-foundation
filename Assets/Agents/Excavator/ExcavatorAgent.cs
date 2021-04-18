// using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class ExcavatorAgent : Agent
{
    [Header("Dependencies")]
    [SerializeField] private Spawner spawner;
    [SerializeField] private Transform target;
    [SerializeField] private EnvironmentData env;
    [SerializeField] private Rigidbody agentRigidbody;
    [SerializeField] private VehicleController controller;

    [SerializeField] private bool keepHistory = false;

    private bool atTarget;
    private float closestDistanceYet;
    private float closestDistanceYetSq;

    // TODO: Make more robust (direct reference to environment)
    private Vector3 environmentOrigin;
    private float stoppedVelocity = 0.5f;
    private float stoppedVelocitySq;
    private bool hasStopped;
    private float initialDistance;
    [System.NonSerialized] public float maxCumulativeReward = 10f;
    [System.NonSerialized] public float minCumulativeReward = -10f;

    private float maxDistanceFromStart;
    private float maxDistanceFromStartSq;
    private float maxVectorMagnitude = 25f;
    private int stepSafetyBuffer = 10;
    private bool initialized = false;
    private List<float> rewardHistory = new List<float>();
    private int stepTotal = 0;

    public float Speed => agentRigidbody.transform.InverseTransformDirection(agentRigidbody.velocity).z;
    public int StepTotal => stepTotal;

    // [Observable]
    private float TargetDistance => target != null ? (transform.position - target.position).magnitude : 0;
    const int REWARD_HISTORY_LENGTH = 10;
    public List<float> RewardHistory { get => rewardHistory; }

    private void Awake()
    {
        if (agentRigidbody == null) agentRigidbody = GetComponent<Rigidbody>();
        if (controller == null) controller = GetComponent<VehicleController>();
        if (target == null) target = transform.parent.Find("Target");

        maxDistanceFromStart = 1.41f * 1.05f * env.Size / 2;
        maxDistanceFromStartSq = maxDistanceFromStart * maxDistanceFromStart;

        stoppedVelocitySq = stoppedVelocity * stoppedVelocity;
    }

    private void Start()
    {
        // TODO: Make more robust (direct reference to environment)
        environmentOrigin = transform.parent.position;
    }

    public override void OnEpisodeBegin()
    {
        controller.ResetController();
        initialized = false;
        MaxStep = env.MaxSteps;
        spawner.Spawn();
    }

    private void Init()
    {
        if (initialized) return;
        closestDistanceYet = TargetDistance;
        initialDistance = TargetDistance;
        initialized = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 targetVector = transform.RelativeVectorTo(target.position);
        if (targetVector.magnitude > maxVectorMagnitude)
        {
            // Cap vector length
            targetVector = targetVector.normalized * maxVectorMagnitude;
        }
        targetVector = targetVector / maxVectorMagnitude; // Normalized
        sensor.AddObservation(targetVector / maxVectorMagnitude); // Existential penalty
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (
            collision.collider.tag != "Terrain"
            && StepCount > stepSafetyBuffer
        )
        {
            var speedSelf = controller.Speed;
            var maxSpeed = controller.maxSpeed;

            float force = collision.impulse.magnitude / Time.fixedDeltaTime;
            float penalty = 0f;
            float threshold = 100f;
            float forcePerPenaltyPoint = 1000f;

            if (force > threshold)
            {
                penalty = (force - threshold) * (1 / forcePerPenaltyPoint);
            }

            AddReward(-penalty);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target")) atTarget = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Target")) atTarget = false;
    }

    private void FixedUpdate()
    {
        stepTotal++;
        if (StepCount > stepSafetyBuffer)
        {
            Init();

            hasStopped = agentRigidbody.velocity.sqrMagnitude < stoppedVelocitySq;
            var success = hasStopped & atTarget;

            var agentBelowGround = transform.position.y < environmentOrigin.y - 15f;
            var agentOutsideArea = (transform.position - environmentOrigin).sqrMagnitude > maxDistanceFromStartSq;

            // var targetBelowGround = target.transform.position.y < environmentOrigin.y - 15f;
            // var targetOutsideArea = (target.transform.position - environmentOrigin).sqrMagnitude > maxDistanceFromStartSq;

            var error =
                agentBelowGround
                | agentOutsideArea;
            // | targetBelowGround
            // | targetOutsideArea;

            var episodeShouldEnd = error | success;

            if (TargetDistance < closestDistanceYet)
            {
                // Debug.Log(
                //     "goal: " + TargetDistance.ToString("F1") + "m"
                //      + " " + "best: " + closestDistanceYet.ToString("F1") + "m"
                //      + " " + "init: " + initialDistance.ToString("F1") + "m"
                // );
                AddReward(2f * (closestDistanceYet - TargetDistance) / initialDistance);
                closestDistanceYet = TargetDistance;
            }

            if (episodeShouldEnd)
            {
                AddReward(success ? 4f : 0);
                RecordReward();
                EndEpisode();
            }

            // Making the agent restless
            AddReward(-1f / (MaxStep - stepSafetyBuffer) * (hasStopped & !atTarget ? 1f + env.Difficulty : 0.1f));

            if (StepCount == MaxStep - 1) RecordReward();
        }
    }

    void RecordReward()
    {
        if (!keepHistory) return;
        rewardHistory.Add(GetCumulativeReward());
        if (rewardHistory.Count > REWARD_HISTORY_LENGTH)
        {
            rewardHistory.RemoveAt(0);
        }
    }
}