using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class GeneralAgent : Agent {
  public bool logObservations = true;

  public override void OnActionReceived(ActionBuffers actions) {
    if (!logObservations) return;
    foreach (var observation in GetObservations()) {
      Debug.Log(observation);
    }
  }
}
