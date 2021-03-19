#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Config))]
public class ConfigEditor : Editor {
  bool showOutput = true;
  bool showHelp = true;
  string trainer;

  public override void OnInspectorGUI() {
    base.OnInspectorGUI();
    Config config = (Config)target;

    showHelp = EditorGUILayout.Toggle("Keep Help Visible", showHelp);
    EditorGUILayout.Space(10);

    EditorGUI.BeginChangeCheck();
    DisplayEntries(config.entries);
    if (EditorGUI.EndChangeCheck()) config.WriteFile();

    GUILayout.Space(10);
    DisplayOutput(config);
  }

  private void DisplayOutput(Config config) {
    showOutput = EditorGUILayout.Foldout(showOutput, "YAML Output");
    if (showOutput) {
      if (GUILayout.Button("Open File")) {
        EditorUtility.OpenWithDefaultApp(config.fileLocation + config.configName + ".yaml");
      }
      EditorGUILayout.HelpBox(config.ReadFile(), MessageType.None);
    }
  }

  public void DisplayEntries(List<Config.Entry> entries) {
    bool singleLineSpace = false;
    foreach (var entry in entries) {
      if (entry.key == "trainer_type") trainer = (string)entry.value;
      if (entry.entries != null && entry.entries.Count > 0) {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField(ToDirtyTitleCase(entry.label), EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        DisplayEntries(entry.entries);
        EditorGUI.indentLevel--;
        singleLineSpace = true;
      } else {
        if (singleLineSpace) EditorGUILayout.Space(10);
        DisplayEntry(entry);
        singleLineSpace = false;
      }
    }
  }

  public void DisplayEntry(Config.Entry entry) {
    bool notTrainerField = entry.key != "trainer_type";
    bool wrongTrainer = !entry.trainers.Contains(trainer);

    EditorGUILayout.BeginHorizontal();

    EditorGUI.BeginDisabledGroup((!entry.active || wrongTrainer) && notTrainerField);
    var content = new GUIContent(ToDirtyTitleCase(entry.label), showHelp ? null : entry.help);
    EditorGUILayout.PrefixLabel(content);
    switch (entry.value.GetType().ToString()) {
      case ("System.String"):
        entry.value = EditorGUILayout.TextField(entry.value.ToString());
        break;
      case ("System.Int32"):
        entry.value = EditorGUILayout.IntField((int)entry.value);
        break;
      case ("System.Single"):
        entry.value = EditorGUILayout.FloatField((float)entry.value);
        break;
      case ("System.Boolean"):
        entry.value = EditorGUILayout.Toggle((bool)entry.value);
        break;
      default:
        Debug.Log(ToDirtyTitleCase(entry.label) + " is type " + entry.value.GetType().ToString());
        break;
    }
    EditorGUI.EndDisabledGroup();

    if (notTrainerField) {
      EditorGUI.BeginDisabledGroup(wrongTrainer);
      int temp = EditorGUI.indentLevel;
      EditorGUI.indentLevel = 0;
      entry.active = EditorGUILayout.Toggle(entry.active, GUILayout.Width(15));
      EditorGUI.indentLevel = temp;
      EditorGUI.EndDisabledGroup();
    }

    EditorGUILayout.EndHorizontal();

    if (entry.help != null && showHelp) {
      EditorGUILayout.HelpBox(entry.help, MessageType.None);
      EditorGUILayout.Space(5);
    };
  }

  string ToDirtyTitleCase(string text) {
    string[] words = text.Split('_');
    text = "";
    List<string> allLower = new List<string> {
      "of",
      "the",
      "in",
      "and",
      "to",
      "per"
    };
    List<string> allUpper = new List<string> {
      "ID",
      "YAML"
    };
    bool isFirst = true;

    foreach (var word in words) {
      if (isFirst) {
        isFirst = false;
      } else {
        text += " ";
      }

      if (word.Length <= 0) continue;

      if (allUpper.Contains(word.ToUpper())) {
        text += word.ToUpper();
      } else if (allLower.Contains(word.ToLower())) {
        text += word.ToLower();
      } else {
        text += word[0].ToString().ToUpper() + word.Substring(1);
      }
    }

    return text;
  }
}
#endif