using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class VehicleAgent : GeneralAgent {
  public Transform target;

  public override void CollectObservations(VectorSensor sensor) {
    sensor.AddObservation(RelativeTargetVector());
  }

  private Vector3 RelativeTargetVector() {
    return (Quaternion.Inverse(transform.rotation) * (target.transform.position - transform.position));
  }
}
