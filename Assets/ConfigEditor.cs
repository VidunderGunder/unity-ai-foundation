#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Config))]
public class ConfigEditor : Editor
{
    private Config config;

    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();
        config = (Config) target;

        if (GUILayout.Button("Create New Config File"))
            config.path = EditorUtility.SaveFilePanelInProject("Initialize Config File", "config", "yaml", "");
        EditorGUILayout.ObjectField(
            AssetDatabase.LoadAssetAtPath<TextAsset>(config.path),
            typeof(TextAsset),
            false
        );

        if (config.fileExists)
        {
            EditorGUI.BeginChangeCheck();
            config.disableYAMLOutput = EditorGUILayout.Toggle("Disable YAML-Output", config.disableYAMLOutput);
            config.showHelp = EditorGUILayout.Toggle("Always Show Tooltips", config.showHelp);
            EditorGUILayout.Space(10);

            DisplayEntries(config.entries);
            if (EditorGUI.EndChangeCheck() && !config.disableYAMLOutput) config.WriteFile();
            ;

            // Refresh data
            GUILayout.Space(10);
            DisplayOutput(config, config.path);
        }
        else
        {
            EditorGUILayout.HelpBox("Add a config file to enable editing :)", MessageType.None);
        }
    }

    private void DisplayOutput(Config config, string path)
    {
        config.showOutput = EditorGUILayout.Foldout(config.showOutput, "YAML Output");
        if (config.showOutput)
        {
            if (GUILayout.Button("Open File")) EditorUtility.OpenWithDefaultApp(path);
            EditorGUILayout.HelpBox(config.ReadFile(), MessageType.None);
        }
    }

    public void DisplayEntries(Config.Entries entries)
    {
        var singleLineSpace = false;
        foreach (var entry in entries.Values)
            if (entry.entries != null && entry.entries.Count > 0)
            {
                EditorGUILayout.Space(10);
                if (DisplayEntry(entry, true))
                {
                    EditorGUI.indentLevel++;
                    DisplayEntries(entry.entries);
                    EditorGUI.indentLevel--;
                };
                singleLineSpace = true;
            }
            else
            {
                if (singleLineSpace) EditorGUILayout.Space(10);
                DisplayEntry(entry);
                singleLineSpace = false;
            }
    }

    public bool DisplayEntry(Config.Entry entry, bool isLabel = false)
    {
        var notTrainerField = entry.key != "trainer_type";
        var wrongTrainer = !entry.trainers.Contains(config.trainer);
        var excludeEntry = !entry.active | wrongTrainer && notTrainerField;

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(excludeEntry);
        var content = new GUIContent(ToDirtyTitleCase(entry.label), config.showHelp ? null : entry.help);
        if (isLabel)
        {
            EditorGUILayout.LabelField(content, EditorStyles.boldLabel);
        }
        else
        {
            EditorGUILayout.PrefixLabel(content);
            switch (entry.value.GetType().ToString())
            {
                case "System.String":
                    entry.value = EditorGUILayout.TextField(entry.value.ToString());
                    break;
                case "System.Int32":
                    entry.value = EditorGUILayout.IntField((int) entry.value);
                    break;
                case "System.Single":
                    entry.value = EditorGUILayout.FloatField((float) entry.value);
                    break;
                case "System.Boolean":
                    entry.value = EditorGUILayout.Toggle((bool) entry.value);
                    break;
                default:
                    break;
            }
        }

        EditorGUI.EndDisabledGroup();

        if (notTrainerField)
        {
            EditorGUI.BeginDisabledGroup(wrongTrainer);
            var temp = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            entry.active = EditorGUILayout.Toggle(entry.active, GUILayout.Width(15));
            EditorGUI.indentLevel = temp;
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.EndHorizontal();

        if (entry.help != null && config.showHelp)
        {
            EditorGUILayout.HelpBox(entry.help, MessageType.None);
            EditorGUILayout.Space(5);
        }

        return !excludeEntry;
    }

    private string ToDirtyTitleCase(string text)
    {
        var words = text.Split('_');
        text = "";
        var allLower = new List<string>
        {
            "of",
            "the",
            "in",
            "and",
            "to",
            "per"
        };
        var allUpper = new List<string>
        {
            "ID",
            "YAML"
        };
        var isFirst = true;

        foreach (var word in words)
        {
            if (isFirst)
                isFirst = false;
            else
                text += " ";

            if (word.Length <= 0) continue;

            if (allUpper.Contains(word.ToUpper()))
                text += word.ToUpper();
            else if (allLower.Contains(word.ToLower()))
                text += word.ToLower();
            else
                text += word[0].ToString().ToUpper() + word.Substring(1);
        }

        return text;
    }
}
#endif