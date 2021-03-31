using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class Training : MonoBehaviour
{
    public bool autoStepsAndDifficulty = false;
    [Tooltip("Period in seconds to increase steps and difficulty automatically")]
    public float increasePeriod = 60 * 60f;
    float difficultyIncrement = 0.01f;
    public int maxStepInitial = 350;
    public int maxStepMax = 5000;
    public Object MLAgentsShellScript;
    public EnvironmentData env;

    [HideInInspector]
    public bool lockGameFocus = false;

    private void Start()
    {
        if (!Application.isPlaying)
        {
            env.isTraining = false;
        }
        if (env.isTraining && autoStepsAndDifficulty)
        {
            StartCoroutine(GraduallyIncreaseDifficulty());
        }
    }

    private IEnumerator GraduallyIncreaseDifficulty()
    {
        float secondsPerIncrement = increasePeriod * difficultyIncrement;
        float maxStepIncrementFloat = (maxStepMax - maxStepInitial) * difficultyIncrement;
        int maxStepIncrement = (int) maxStepIncrementFloat;
        env.MaxSteps = maxStepInitial;

        env.SetDifficulty(0);
        yield return new WaitForSecondsRealtime(secondsPerIncrement);

        while (env.Difficulty < 1f)
        {
            env.MaxSteps += maxStepIncrement;
            env.SetDifficulty(env.Difficulty + difficultyIncrement);
            yield return new WaitForSecondsRealtime(secondsPerIncrement);
        }

        env.MaxSteps = maxStepMax;
        env.SetDifficulty(1f);
    }

    private void OnApplicationFocus()
    {
        // Force game focus when playing to not disable Agent's Input Actuator when training
        if (lockGameFocus && EditorApplication.isPlaying) EditorApplication.ExecuteMenuItem("Window/General/Game");
    }
}
