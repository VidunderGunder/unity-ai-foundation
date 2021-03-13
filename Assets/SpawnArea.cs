using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SpawnArea : MonoBehaviour {
  public EnvironmentData env;
  public List<Transform> stages;

  public void OnDifficultyChange() {
    LerpTransformByDifficulty();
  }

  private void LerpTransformByDifficulty() {
    if (stages.Count.Equals(0)) return;
    if (env.difficulty == 0) {
      transform.position = stages[0].position;
      transform.rotation = stages[0].rotation;
      transform.localScale = stages[0].lossyScale;
      return;
    }
    if (env.difficulty == 1f) {
      transform.position = stages[stages.Count - 1].position;
      transform.rotation = stages[stages.Count - 1].rotation;
      transform.localScale = stages[stages.Count - 1].lossyScale;
      return;
    }

    // Eliminate input range deadzone for the last stage
    float squishedDifficulty = env.difficulty * ((stages.Count - 1f) / (stages.Count));

    int fromStage = (int)(squishedDifficulty * stages.Count);
    if (fromStage == stages.Count) {
      transform.position = stages[stages.Count - 1].position;
      transform.rotation = stages[stages.Count - 1].rotation;
      transform.rotation = stages[stages.Count - 1].rotation;
      return;
    }

    int toStage = fromStage + 1;
    float difficultyPerStage = 1f / stages.Count; // <= 1f
    float progressThroughCurrentStage = (squishedDifficulty - difficultyPerStage * fromStage) * stages.Count; // in range 0-1f

    Debug.Log(" ");
    Debug.Log("SPAWN AREA DEBUG");
    Debug.Log("-------------------------------");
    Debug.Log("fromStage: " + fromStage);
    Debug.Log("toStage: " + toStage);
    Debug.Log("difficultyPerStage: " + difficultyPerStage);
    Debug.Log("progressThroughCurrentStage: " + progressThroughCurrentStage);
    Debug.Log("-------------------------------");
    Debug.Log(" ");

    transform.position = Vector3.Lerp(
        stages[fromStage].position,
        stages[toStage].position,
        progressThroughCurrentStage
    );
    transform.localScale = Vector3.Lerp(
        stages[fromStage].lossyScale,
        stages[toStage].lossyScale,
        progressThroughCurrentStage
    );
    transform.localRotation = Quaternion.Slerp(
        stages[fromStage].localRotation,
        stages[toStage].localRotation,
        progressThroughCurrentStage
    );
  }

  // TODO: Activate on change, not in Update()
  private void Update() {
    OnDifficultyChange();
  }
}
