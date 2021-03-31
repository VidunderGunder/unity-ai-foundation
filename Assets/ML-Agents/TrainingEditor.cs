using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Training))]
public class TrainingEditor : Editor
{
    Training training;
    SerializedObject serializedTraining;
    SerializedObject serializedEnv;

    private void OnEnable()
    {
        training = (Training) target;
        if (!Application.isPlaying) training.lockGameFocus = false;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.HelpBox(
            "Add a shortcut or script that launches training in a terminal with the following command:\n\n"
            + "mlagents-learn config.yaml\n\n"
            + "Make sure it starts in the folder ML-Agents and that you have installed all dependencies.",
            MessageType.None
        );

        training.MLAgentsShellScript = (Object) EditorGUILayout.ObjectField(
            training.MLAgentsShellScript,
            typeof(Object),
            false
        );

        EditorGUI.BeginDisabledGroup(training.env.isTraining);
        if (GUILayout.Button("Train")) Train();
        EditorGUI.EndDisabledGroup();
    }

    public void Train()
    {
        training.lockGameFocus = true;
        training.env.isTraining = true;
        EditorUtility.OpenWithDefaultApp(
            AssetDatabase.GetAssetPath(training.MLAgentsShellScript)
        );
        EditorApplication.isPlaying = true;
    }
}