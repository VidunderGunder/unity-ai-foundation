using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(Training))]
public class TrainingEditor : Editor
{
    Training training;

    private void OnEnable()
    {
        training = (Training) target;
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

        if (GUILayout.Button("Train")) Train();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(training);
            EditorSceneManager.MarkSceneDirty(training.gameObject.scene);
        }
    }

    public void Train()
    {
        EditorUtility.OpenWithDefaultApp(
            AssetDatabase.GetAssetPath(training.MLAgentsShellScript)
        );
        EditorApplication.isPlaying = true;
    }

}