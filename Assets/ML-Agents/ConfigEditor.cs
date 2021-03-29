#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(Config))]
public class ConfigEditor : Editor
{
    private Config config;
    private bool changed = false;

    private void OnEnable()
    {
        config = (Config) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        if (GUILayout.Button("yaml read"))
        {
            var yaml = config.ReadFile();
            Debug.Log(yaml);
        };

        if (config.FileExists)
        {
            OpenFileButton();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            // SaveFileButton();

            Space();

            AllFields();
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            if (GUILayout.Button("Generate Config File"))
            {
                File.WriteAllText(
                    config.Path,
                    string.Empty
                );
            };
            EditorGUILayout.HelpBox(
                "Add a config file to enable editing :)\n\nSadly, you can't yet load a YAML file, only write.",
                MessageType.None
            );
        }

    }

    private void OnLostFocus()
    {
        Debug.Log("Write");
        config.WriteFile();
    }

    private void OpenFileButton()
    {
        if (GUILayout.Button("Open File")) EditorUtility.OpenWithDefaultApp(config.Path);
    }

    public void OnSave()
    {
        if (changed)
        {
            Undo.ClearUndo(target);
            config.WriteFile();
            changed = false;
        }
    }

    // private void SaveFileButton()
    // {
    //     EditorGUI.BeginDisabledGroup(!changed);
    //     if (
    //         GUILayout.Button(
    //             "Apply to YAML-file"
    //         )
    //     )
    //     {
    //         OnSave();
    //     };
    //     EditorGUI.EndDisabledGroup();
    //     EditorGUILayout.LabelField(
    //         (
    //             changed
    //             ? "(Unsaved Changes)"
    //             : ""
    //         ),
    //         EditorStyles.centeredGreyMiniLabel
    //     );
    // }
    private void DefaultFields(List<Config.Entry> entries, string trainer_type = null)
    {
        foreach (var entry in entries)
        {
            DefaultField(entry, trainer_type);
        }
    }
    private void BehaviourFields(Config.TrainerSettings behaviour)
    {
        var trainer_type = behaviour.trainer_settings[0].value;
        EditorGUI.indentLevel++;
        DefaultFields(behaviour.trainer_settings);

        Space();

        DefaultSettingsHeader(behaviour.hyperparameters);
        EditorGUI.indentLevel++;
        DefaultFields(behaviour.hyperparameters.hyperparameters_entries_base);
        Space();
        EditorGUILayout.LabelField("PPO Only", EditorStyles.miniBoldLabel);
        DefaultFields(behaviour.hyperparameters.hyperparameters_entries_ppo, trainer_type);
        Space();
        EditorGUILayout.LabelField("SAC Only", EditorStyles.miniBoldLabel);
        DefaultFields(behaviour.hyperparameters.hyperparameters_entries_sac, trainer_type);

        EditorGUI.indentLevel--;

        Space();

        DefaultSettingsHeader(behaviour.network_settings);
        EditorGUI.indentLevel++;
        DefaultFields(behaviour.network_settings.network_settings_entries);
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;

        Space();
    }

    private void AllFields()
    {

        DefaultSettingsHeader(config.defaultSettings);
        BehaviourFields(config.defaultSettings);

        DefaultSettingsHeader(config.multipleBehaviours);
        int index = 0;
        foreach (var behavior in config.multipleBehaviours.behaviors)
        {
            EditorGUI.indentLevel++;
            DefaultField(behavior.behavior_name);
            BehaviourFields(behavior.Behaviour);
            EditorGUI.indentLevel--;
            if (GUILayout.Button("Remove Behaviour"))
            {
                config.multipleBehaviours.behaviors.RemoveAt(index);
                changed = true;
                OnSave();
            };
            Space();
        }
        if (GUILayout.Button("Add Behaviour"))
        {
            config.multipleBehaviours.behaviors.Add(new Config.TrainerSettingsMultipleBehaviour("behavior_name" + (config.multipleBehaviours.behaviors.Count + 1)));
            changed = true;
            OnSave();
        };
        index++;
        DefaultSettingsHeader(config.checkpointSettings);
        EditorGUI.indentLevel++;
        DefaultFields(config.checkpointSettings.checkpoint_settings);
        EditorGUI.indentLevel--;

        Space();

        DefaultSettingsHeader(config.engineSettings);
        EditorGUI.indentLevel++;
        DefaultFields(config.engineSettings.engine_settings);
        EditorGUI.indentLevel--;

        Space();

        DefaultSettingsHeader(config.torchSettings);
        EditorGUI.indentLevel++;
        DefaultField(config.torchSettings.device);
        EditorGUI.indentLevel--;

        Space();

        DefaultField(config.debug);
    }

    public void Space(int pixels = 5)
    {
        EditorGUILayout.Space(5);
    }

    public void DefaultSettingsHeader(Config.Settings settings)
    {
        var content = new GUIContent(
            settings.Key.ToDirtyTitleCase(),
            config.alwaysShowTooltips
                ? null
                : settings.Help
        );

        EditorGUILayout.LabelField(content, EditorStyles.boldLabel);
        if (
            config.alwaysShowTooltips
            && settings.Help != null
            && settings.Help != "")
        {
            EditorGUILayout.HelpBox(settings.Help, MessageType.None);
        }
    }

    public bool DefaultField(Config.Entry entry, string trainer_type = null)
    {
        var wrongTrainer = false;
        if (trainer_type != null)
        {
            wrongTrainer = !entry.Trainers.Contains((string) trainer_type);
        }
        var excludeEntry = !entry.AlwaysActive && !entry.active | wrongTrainer;

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(excludeEntry);

        var content = new GUIContent(entry.Label.ToDirtyTitleCase(), config.alwaysShowTooltips ? null : entry.Help);
        EditorGUILayout.PrefixLabel(content);

        EditorGUI.BeginChangeCheck();
        string value = null;
        switch (entry.type.Name)
        {
            case nameof(System.String):
                if (entry.Options == null || entry.Options.Length == 0) value = EditorGUILayout.TextField(entry.value);
                else
                {
                    entry.selectedIndex = EditorGUILayout.Popup(entry.selectedIndex, entry.Options);
                    value = entry.Options[entry.selectedIndex];
                }
                break;
            case nameof(System.Int32):
                if (entry.RangeInt == null) value = EditorGUILayout.IntField(System.Int32.Parse(entry.value)).ToString();
                else value = EditorGUILayout.IntSlider(System.Int32.Parse(entry.value), entry.RangeInt.Item1, entry.RangeInt.Item2).ToString();
                break;
            case nameof(System.Single):
                if (entry.RangeFloat == null) value = EditorGUILayout.FloatField(System.Single.Parse(entry.value)).ToString();
                else value = EditorGUILayout.Slider(System.Single.Parse(entry.value), entry.RangeFloat.Item1, entry.RangeFloat.Item2).ToString();
                break;
            case nameof(System.Boolean):
                value = EditorGUILayout.Toggle(System.Boolean.Parse(entry.value)).ToString();
                break;
            default:
                break;
        }


        EditorGUI.EndDisabledGroup();

        bool active = true;

        if (!entry.AlwaysActive)
        {
            EditorGUI.BeginDisabledGroup(wrongTrainer);
            var temp = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            active = EditorGUILayout.Toggle(entry.active, GUILayout.Width(15));
            EditorGUI.indentLevel = temp;
            EditorGUI.EndDisabledGroup();
        }

        if (EditorGUI.EndChangeCheck())
        {
            // Debug.Log(value);
            Undo.RecordObject(target, "Changed Trainer Config");
            if (value != null)
            {
                entry.value = value;
                changed = true;
            };
            if (active != entry.active)
            {
                entry.active = active;
                changed = true;
            };
            OnSave();
        }

        EditorGUILayout.EndHorizontal();

        if (entry.Help != null && entry.Help != "" && config.alwaysShowTooltips)
        {
            EditorGUILayout.HelpBox(entry.Help, MessageType.None);
            EditorGUILayout.Space(5);
        }

        return !excludeEntry;
    }
}
#endif