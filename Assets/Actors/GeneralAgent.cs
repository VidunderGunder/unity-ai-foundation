using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class GeneralAgent : Agent {
  public bool logObservations = true;

  public override void OnEpisodeBegin() {

  }

  public override void CollectObservations(VectorSensor sensor) {

  }

  public override void OnActionReceived(ActionBuffers actions) {
    if (!logObservations) return;
    foreach (var observation in GetObservations()) {
      Debug.Log(observation);
    }

  }

  public virtual void Reward() { }
}
