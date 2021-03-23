using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SpawnArea : MonoBehaviour
{
    public EnvironmentData env;
    public List<Transform> stages;

    public void OnDifficultyChange()
    {
        LerpTransformByDifficulty();
    }

    private void LerpTransformByDifficulty()
    {
        if (stages.Count.Equals(0)) return;

        if (stages.Count.Equals(1))
        {
            transform.position = stages[0].position;
            transform.localScale = stages[0].lossyScale;
            transform.localRotation = stages[0].localRotation;
            return;
        }

        ;

        if (env.Difficulty == 0)
        {
            transform.position = stages[0].position;
            transform.rotation = stages[0].rotation;
            transform.localScale = stages[0].lossyScale;
            return;
        }

        if (env.Difficulty == 1f)
        {
            transform.position = stages[stages.Count - 1].position;
            transform.rotation = stages[stages.Count - 1].rotation;
            transform.localScale = stages[stages.Count - 1].lossyScale;
            return;
        }

        // Eliminate input range deadzone for the last stage
        var squishedDifficulty = env.Difficulty * ((stages.Count - 1f) / stages.Count);

        var fromStage = (int) (squishedDifficulty * stages.Count);
        if (fromStage == stages.Count)
        {
            transform.position = stages[stages.Count - 1].position;
            transform.rotation = stages[stages.Count - 1].rotation;
            transform.rotation = stages[stages.Count - 1].rotation;
            return;
        }

        var toStage = fromStage + 1;
        var difficultyPerStage = 1f / stages.Count; // <= 1f
        var progressThroughCurrentStage =
            (squishedDifficulty - difficultyPerStage * fromStage) * stages.Count; // in range 0-1f

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

    // TODO: Activate on difficulty change, not in Update()
    private void Update()
    {
        OnDifficultyChange();
    }
}