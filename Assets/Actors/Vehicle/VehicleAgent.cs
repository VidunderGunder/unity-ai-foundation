// using System;

using System;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class VehicleAgent : GeneralAgent
{
    private readonly float maxCumulativeCollisionPenalty = 2f;

    private readonly float maxSingleCollisionPenalty = 1.5f;
    [SerializeField] private Rigidbody agentRigidbody;
    private bool atTarget;
    private float closestDistanceSq;
    private float cumulativeCollisionPenalty;
    [SerializeField] private EnvironmentData env;

    // TODO: Make more robust (direct reference to environment)
    private Vector3 environmentOrigin;
    private bool hasStopped;
    private float initialDistanceSq;
    [NonSerialized] public float maxCumulativeReward = 3f;

    private float maxDistanceFromStart;

    private float maxDistanceFromStartSq;
    // public Transform target;

    // public override void CollectObservations(VectorSensor sensor) {
    //   sensor.AddObservation(transform.RelativeVectorTo(target.position)); // Existential penalty
    // }

    [NonSerialized] public float minCumulativeReward = -2f;

    [Header("Dependencies")] [SerializeField]
    private Spawner spawner;

    [SerializeField] private Transform target;

    // [Observable]
    private float targetDistanceSq => target != null ? (transform.position - target.position).sqrMagnitude : 0;

    private void Awake()
    {
        if (agentRigidbody == null) agentRigidbody = GetComponent<Rigidbody>();
        if (target == null) target = transform.parent.Find("Target");

        maxDistanceFromStart = 1.41f * 1.05f * env.size / 2;
        maxDistanceFromStartSq = maxDistanceFromStart * maxDistanceFromStart;
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
        closestDistanceSq = targetDistanceSq;
        initialDistanceSq = targetDistanceSq;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.RelativeVectorTo(target.position)); // Existential penalty
        AddReward(-1f / MaxStep * (hasStopped & !atTarget ? 1f + env.difficulty : 0.1f)); // Existential penalty
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
        hasStopped = agentRigidbody.velocity.sqrMagnitude < 0.0002f;
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

        if (!atTarget & (targetDistanceSq < closestDistanceSq))
        {
            AddReward((closestDistanceSq - targetDistanceSq) / initialDistanceSq);
            closestDistanceSq = targetDistanceSq;
        }

        if (episodeShouldEnd)
        {
            AddReward(success ? 1f : 0);
            EndEpisode();
        }
    }
}