using System.Collections;
using UnityEditor;
using UnityEngine;

public class Training : MonoBehaviour
{
    [HideInInspector]
    public Object MLAgentsShellScript;

    private void OnApplicationFocus(bool focusStatus)
    {
        // Force game focus when playing to not disable Agent's Input Actuator when training
        if (EditorApplication.isPlaying) EditorApplication.ExecuteMenuItem("Window/General/Game");
    }
}

