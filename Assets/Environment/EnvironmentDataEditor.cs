using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(EnvironmentData))]
public class EnvironmentDataEditor : Editor
{
    EnvironmentData env;

    private void OnEnable()
    {
        env = (EnvironmentData) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        env.Instances = EditorGUILayout.Vector3IntField("Instances", env.Instances);

        if (GUI.changed)
        {
            if (Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
            EditorUtility.SetDirty(env);
        }
    }
}
