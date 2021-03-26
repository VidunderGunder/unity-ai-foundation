// using System;

using Unity.MLAgents.Sensors;
using UnityEngine;

public class VehicleAgent : GeneralAgent
{
    private readonly float maxCumulativeCollisionPenalty = 2f;
    private readonly float maxSingleCollisionPenalty = 1.5f;
    [SerializeField] private Rigidbody agentRigidbody;
    private bool atTarget;
    private float closestDistanceYet;
    private float closestDistanceYetSq;
    private float cumulativeCollisionPenalty;
    [SerializeField] private EnvironmentData env;

    // TODO: Make more robust (direct reference to environment)
    private Vector3 environmentOrigin;
    private float stoppedVelocity;
    private float stoppedVelocitySq;
    private bool hasStopped;
    private float initialDistance;
    [System.NonSerialized] public float maxCumulativeReward = 5f;
    [System.NonSerialized] public float minCumulativeReward = -2f;

    private float maxDistanceFromStart;
    private float maxDistanceFromStartSq;
    private float maxVectorMagnitude = 75f;

    [Header("Dependencies")]
    [SerializeField] private Spawner spawner;
    [SerializeField] private Transform target;

    // [Observable]
    private float TargetDistance => target != null ? (transform.position - target.position).magnitude : 0;

    private void Awake()
    {
        if (agentRigidbody == null) agentRigidbody = GetComponent<Rigidbody>();
        if (target == null) target = transform.parent.Find("Target");

        maxDistanceFromStart = 1.41f * 1.05f * env.Size / 2;
        maxDistanceFromStartSq = maxDistanceFromStart * maxDistanceFromStart;

        stoppedVelocity = 0.5f;
        stoppedVelocitySq = stoppedVelocity * stoppedVelocity;
    }

    private void Start()
    {
        // TODO: Make more robust (direct reference to environment)
        environmentOrigin = transform.parent.position;
    }

    public override void OnEpisodeBegin()
    {
        spawner.Spawn();
        cumulativeCollisionPenalty = 0;

        closestDistanceYet = TargetDistance;
        initialDistance = TargetDistance;
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

        AddReward(-1f / MaxStep * (hasStopped & !atTarget ? 1f + env.Difficulty : 0.1f)); // Existential penalty
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (
            collision.collider.tag != "Terrain"
        )
        {
            var collisionPenalty = collision.relativeVelocity.sqrMagnitude * 0.025f;
            collisionPenalty = collisionPenalty > maxSingleCollisionPenalty
                ? maxSingleCollisionPenalty
                : collisionPenalty;
            cumulativeCollisionPenalty += collisionPenalty;
            if (cumulativeCollisionPenalty > maxCumulativeCollisionPenalty)
            {
                collisionPenalty = maxCumulativeCollisionPenalty - cumulativeCollisionPenalty;
                cumulativeCollisionPenalty = maxCumulativeCollisionPenalty;
            }

            AddReward(-collisionPenalty);
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
        hasStopped = agentRigidbody.velocity.sqrMagnitude < stoppedVelocitySq;
        var success = hasStopped & atTarget;

        var agentBelowGround = transform.position.y < environmentOrigin.y - 15f;
        var agentOutsideArea = (transform.position - environmentOrigin).sqrMagnitude > maxDistanceFromStartSq;

        var targetBelowGround = target.transform.position.y < environmentOrigin.y - 15f;
        var targetOutsideArea = (target.transform.position - environmentOrigin).sqrMagnitude > maxDistanceFromStartSq;

        var error =
            agentBelowGround |
            agentOutsideArea |
            targetBelowGround |
            targetOutsideArea;

        var episodeShouldEnd = error | success;

        if (!atTarget & (TargetDistance < closestDistanceYet))
        {
            AddReward((closestDistanceYet - TargetDistance) / initialDistance);
            closestDistanceYet = TargetDistance;
        }

        if (episodeShouldEnd)
        {
            AddReward(success ? 2f : 0);
            EndEpisode();
        }
    }
}