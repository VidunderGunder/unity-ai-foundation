// using System;
// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
// using Unity.MLAgents;
// using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class VehicleAgent : GeneralAgent {
  public Transform target;

  public override void CollectObservations(VectorSensor sensor) {
    sensor.AddObservation(transform.RelativeVectorTo(target.position));
  }
}
