using System.Collections;
using UnityEditor;
using UnityEngine;

public class Training : MonoBehaviour
{
    public bool lockGameFocus = true;
    [HideInInspector]
    public Object MLAgentsShellScript;

    private void OnApplicationFocus()
    {
        // Force game focus when playing to not disable Agent's Input Actuator when training
        if (lockGameFocus && EditorApplication.isPlaying) EditorApplication.ExecuteMenuItem("Window/General/Game");
    }
}
