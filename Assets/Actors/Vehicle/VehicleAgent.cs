// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class VehicleAgent : GeneralAgent {
  [Header("Dependencies")]
  // public Transform target;

  // public override void CollectObservations(VectorSensor sensor) {
  //   sensor.AddObservation(transform.RelativeVectorTo(target.position)); // Existential penalty
  // }

  [System.NonSerialized] public float minCumulativeReward = -2f;
  [System.NonSerialized] public float maxCumulativeReward = 3f;

  [SerializeField] private Spawner spawner;
  [SerializeField] private EnvironmentData env;
  [SerializeField] private Transform target;
  [SerializeField] private Rigidbody agentRigidbody;

  // TODO: Make more robust (direct reference to environment)
  private Vector3 environmentOrigin;

  private float maxDistanceFromStart;
  private float maxDistanceFromStartSq;
  private bool atTarget;
  private bool hasStopped;
  private float closestDistanceSq;
  private float initialDistanceSq;

  private float maxSingleCollisionPenalty = 1.5f;
  private float maxCumulativeCollisionPenalty = 2f;
  private float cumulativeCollisionPenalty = 0;

  // [Observable]
  float targetDistanceSq {
    get {
      return target != null ? (transform.position - target.position).sqrMagnitude : 0;
    }
  }

  private void Awake() {
    if (agentRigidbody == null) agentRigidbody = GetComponent<Rigidbody>();
    if (target == null) target = transform.parent.Find("Target");

    maxDistanceFromStart = 1.41f * 1.05f * env.size / 2;
    maxDistanceFromStartSq = maxDistanceFromStart * maxDistanceFromStart;
  }

  private void Start() {
    // TODO: Make more robust (direct reference to environment)
    environmentOrigin = transform.parent.position;
  }

  public override void OnEpisodeBegin() {
    spawner.Spawn();
    cumulativeCollisionPenalty = 0;
    closestDistanceSq = targetDistanceSq;
    initialDistanceSq = targetDistanceSq;
  }

  public override void CollectObservations(VectorSensor sensor) {
    sensor.AddObservation(transform.RelativeVectorTo(target.position)); // Existential penalty
    AddReward((-1f / MaxStep) * (hasStopped & !atTarget ? 1f + env.difficulty : 0.1f)); // Existential penalty
  }

  private void OnCollisionEnter(Collision collision) {
    if (
      collision.collider.tag != "Terrain"
    ) {
      float collisionPenalty = collision.relativeVelocity.sqrMagnitude * 0.025f;
      collisionPenalty = collisionPenalty > maxSingleCollisionPenalty ? maxSingleCollisionPenalty : collisionPenalty;
      cumulativeCollisionPenalty += collisionPenalty;
      if (cumulativeCollisionPenalty > maxCumulativeCollisionPenalty) {
        collisionPenalty = maxCumulativeCollisionPenalty - cumulativeCollisionPenalty;
        cumulativeCollisionPenalty = maxCumulativeCollisionPenalty;
      }
      AddReward(-collisionPenalty);
    }
  }

  private void OnTriggerEnter(Collider other) {
    if (other.CompareTag("Target")) {
      atTarget = true;
    }
  }

  private void OnTriggerExit(Collider other) {
    if (other.CompareTag("Target")) {
      atTarget = false;
    }
  }

  private void FixedUpdate() {
    hasStopped = agentRigidbody.velocity.sqrMagnitude < 0.0002f;
    bool success = hasStopped & atTarget;

    bool agentBelowGround = transform.position.y < environmentOrigin.y - 15f;
    bool agentOutsideArea = (transform.position - environmentOrigin).sqrMagnitude > maxDistanceFromStartSq;

    bool targetBelowGround = target.transform.position.y < environmentOrigin.y - 15f;
    bool targetOutsideArea = (target.transform.position - environmentOrigin).sqrMagnitude > maxDistanceFromStartSq;

    bool error =
      agentBelowGround |
      agentOutsideArea |
      targetBelowGround |
      targetOutsideArea;

    bool episodeShouldEnd = error | success;

    if (!atTarget & targetDistanceSq < closestDistanceSq) {
      AddReward((closestDistanceSq - targetDistanceSq) / initialDistanceSq);
      closestDistanceSq = targetDistanceSq;
    }

    if (episodeShouldEnd) {
      AddReward(success ? 1f : 0);
      EndEpisode();
    }
  }
}
