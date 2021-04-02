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
    private bool newFileExists = false;
    private string newFilename;
    private bool alwaysShowTooltips = false;
    private int selectedIndexFile = 0;
    private string[] allExistingFile = new string[] { };

    private void getAllExistingFile()
    {
        var path = config.GetCWD() + "/";
        var info = new DirectoryInfo(path);
        var fileInfo = info.GetFiles("*.yaml");
        allExistingFile = new string[fileInfo.Length + 1];
        allExistingFile[0] = "";
        for (int i = 0; i < fileInfo.Length; i++)
        {
            allExistingFile[i + 1] = Path.GetFileName(fileInfo[i].ToString().Split('.')[0]);
        }
    }

    private void OnEnable()
    {
        config = (Config) target;
        newFilename = config.filename;
        if (config.FileExists)
            config.ReadFile();
    }

    public override void OnInspectorGUI()
    {
        getAllExistingFile();
        base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        newFilename = EditorGUILayout.TextField("Filename", newFilename);
        if (EditorGUI.EndChangeCheck())
            selectedIndexFile = 0;
        newFileExists = File.Exists(config.GetCWD() + "/" + newFilename + ".yaml");

        selectedIndexFile = EditorGUILayout.Popup("Select a Existing Files", selectedIndexFile, allExistingFile);
        var selectednewFilename = allExistingFile[selectedIndexFile];
        if (selectednewFilename != "")
            newFilename = selectednewFilename;


        if (config.FileExists && config.filename == newFilename)
        {
            OpenFileButton();
            alwaysShowTooltips = EditorGUILayout.Toggle("Always Show Tooltips", alwaysShowTooltips);
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            Space();
            AllFields();
            EditorGUI.EndDisabledGroup();
        }
        else if (newFileExists && config.filename != newFilename)
        {
            Space();
            if (GUILayout.Button("Edit " + newFilename))
            {
                config.filename = newFilename;
                config.ReadFile();
            };
            Space();
            if (config.FileExists)
            {
                if (GUILayout.Button("Generate Config File From Current File"))
                {
                    NewFileFromDefault();
                };
                EditorGUILayout.HelpBox(
                "Are you secure that you want to overwrite the existing file",
                MessageType.Warning
            );
            }
            if (GUILayout.Button("Generate Config File From Default Value"))
            {
                NewFileFromFile();
            };
            EditorGUILayout.HelpBox(
                "Are you secure that you want to overwrite the existing file",
                MessageType.Warning
            );

        }
        else if (!newFileExists)
        {
            if (config.FileExists)
            {
                if (GUILayout.Button("Generate Config File Current File"))
                {
                    NewFileFromDefault();
                };
            }
            if (GUILayout.Button("Generate Config File From Default Value"))
            {
                NewFileFromFile();
            };
            EditorGUILayout.HelpBox(
                "Add a config file to enable editing :)\n\nSadly, you can't yet load a YAML file, only write.",
                MessageType.None
            );
        }
        if (config.filename != newFilename)
        {
            Space();
            if (GUILayout.Button("Go back to old filename"))
            {
                newFilename = config.filename;
                selectedIndexFile = 0;
            };
        }
    }

    private void NewFileFromDefault()
    {
        config.filename = newFilename;
        File.WriteAllText(
            config.Path,
            string.Empty
        );
        changed = true;
        OnSave();
    }

    private void NewFileFromFile()
    {
        config.defaultSettings = new Config.TrainerSettings("default_settings", alwaysActive: true);
        config.multipleBehaviours = new Config.MultipleBehaviours("behaviors", alwaysActive: false);
        config.engineSettings = new Config.EngineSettings();
        config.environmentParameters = new Config.EnvironmentParameters();
        config.checkpointSettings = new Config.CheckpointSettings();
        config.torchSettings = new Config.TorchSettings();
        config.filename = newFilename;
        File.WriteAllText(
            config.Path,
            string.Empty
        );
        changed = true;
        OnSave();
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
        InUse(config.defaultSettings);
        if (config.defaultSettings.inUse)
        {
            DefaultSettingsHeader(config.defaultSettings);
            BehaviourFields(config.defaultSettings);
        }

        DefaultSettingsHeader(config.multipleBehaviours);
        var behaviors = config.multipleBehaviours.behaviors;
        for (int i = 0; i < config.multipleBehaviours.behaviors.Count; ++i)
        {
            EditorGUI.indentLevel++;
            DefaultField(behaviors[i].behavior_name);
            BehaviourFields(behaviors[i].Behaviour);
            EditorGUI.indentLevel--;
            if (GUILayout.Button("Remove Behaviour"))
            {
                config.multipleBehaviours.behaviors.RemoveAt(i);
                changed = true;
                OnSave();
            };
            Space();
        };
        if (GUILayout.Button("Add Behaviour"))
        {
            config.multipleBehaviours.behaviors.Add(new Config.TrainerSettingsMultipleBehaviour("behavior name " + (config.multipleBehaviours.behaviors.Count + 1)));
            changed = true;
            OnSave();
        };

        InUse(config.checkpointSettings);
        if (config.checkpointSettings.inUse)
        {
            DefaultSettingsHeader(config.checkpointSettings);
            EditorGUI.indentLevel++;
            DefaultFields(config.checkpointSettings.checkpoint_settings);
            EditorGUI.indentLevel--;
            Space();
        }

        InUse(config.engineSettings);
        if (config.engineSettings.inUse)
        {
            DefaultSettingsHeader(config.engineSettings);
            EditorGUI.indentLevel++;
            DefaultFields(config.engineSettings.engine_settings);
            EditorGUI.indentLevel--;
            Space();
        }
        InUse(config.torchSettings);
        if (config.torchSettings.inUse)
        {
            DefaultSettingsHeader(config.torchSettings);
            EditorGUI.indentLevel++;
            DefaultFields(config.torchSettings.device);
            EditorGUI.indentLevel--;
            Space();
        }

        DefaultField(config.debug);
    }

    private void InUse(Config.Settings settings)
    {
        settings.inUse = EditorGUILayout.Toggle("Use " + settings.Key.ToDirtyTitleCase(), settings.inUse);

    }

    public void Space(int pixels = 5)
    {
        EditorGUILayout.Space(5);
    }

    public void DefaultSettingsHeader(Config.Settings settings)
    {
        var content = new GUIContent(
            settings.Key.ToDirtyTitleCase(),
            alwaysShowTooltips
                ? null
                : settings.Help
        );

        EditorGUILayout.LabelField(content, EditorStyles.boldLabel);
        if (
            alwaysShowTooltips
            && settings.Help != null
            && settings.Help != "")
        {
            EditorGUILayout.HelpBox(settings.Help, MessageType.None);
        }
    }

    private void uniqueBehaviorName(string value)
    {
        int numberWithSameValue = 0;
        var multipleBehaviours = config.multipleBehaviours;
        foreach (var behavior in config.multipleBehaviours.behaviors)
        {
            if (behavior.behavior_name.value == value)
            {
                numberWithSameValue++;
            }
        }
        if (1 < numberWithSameValue)
        {
            EditorGUILayout.HelpBox("Behavior name are not unique", MessageType.Error);
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
        var content = new GUIContent(entry.Label.ToDirtyTitleCase(), alwaysShowTooltips ? null : entry.Help);
        EditorGUIUtility.fieldWidth = 80;

        EditorGUI.BeginChangeCheck();
        string value = null;

        switch (entry.type.Name)
        {
            case nameof(System.String):
                if (entry.Options == null || entry.Options.Length == 0) value = EditorGUILayout.TextField(content, entry.value);
                else
                {
                    entry.selectedIndex = EditorGUILayout.Popup(content, entry.selectedIndex, entry.Options);
                    value = entry.Options[entry.selectedIndex];
                }
                break;
            case nameof(System.Int32):
                if (entry.RangeInt == null) value = EditorGUILayout.IntField(content, System.Int32.Parse(entry.value)).ToString();
                else value = EditorGUILayout.IntSlider(content, System.Int32.Parse(entry.value), entry.RangeInt.Item1, entry.RangeInt.Item2).ToString();
                break;
            case nameof(System.Single):
                if (entry.RangeFloat == null) value = EditorGUILayout.FloatField(content, System.Single.Parse(entry.value)).ToString();
                else value = EditorGUILayout.Slider(content, System.Single.Parse(entry.value), entry.RangeFloat.Item1, entry.RangeFloat.Item2).ToString();
                break;
            case nameof(System.Boolean):
                value = EditorGUILayout.Toggle(content, System.Boolean.Parse(entry.value)).ToString();
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
        EditorGUILayout.EndHorizontal();

        // // Custom 
        switch (entry.Key)
        {
            case "behavior_name":
                uniqueBehaviorName(entry.value);
                break;
            default:
                break;
        }

        if (EditorGUI.EndChangeCheck())
        {
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

        if (entry.Help != null && entry.Help != "" && alwaysShowTooltips)
        {
            EditorGUILayout.HelpBox(entry.Help, MessageType.None);
            EditorGUILayout.Space(5);
        }

        return !excludeEntry;
    }
}
#endif