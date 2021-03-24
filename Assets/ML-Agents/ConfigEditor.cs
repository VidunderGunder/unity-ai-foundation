#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

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

        if (config.FileExists)
        {
            OpenFileButton();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            SaveFileButton();

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

    private void SaveFileButton()
    {
        EditorGUI.BeginDisabledGroup(!changed);
        if (
            GUILayout.Button(
                "Apply to YAML-file"
            )
        )
        {
            OnSave();
        };
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.LabelField(
            (
                changed
                ? "(Unsaved Changes)"
                : ""
            ),
            EditorStyles.centeredGreyMiniLabel
        );
    }

    private void AllFields()
    {
        DefaultSettingsHeader(config.defaultSettings);
        EditorGUI.indentLevel++;
        DefaultField(config.defaultSettings.trainer_type);
        DefaultField(config.defaultSettings.summary_freq);
        DefaultField(config.defaultSettings.max_steps);
        DefaultField(config.defaultSettings.checkpoint_interval);
        DefaultField(config.defaultSettings.keep_checkpoints);
        DefaultField(config.defaultSettings.time_horizon);
        DefaultField(config.defaultSettings.init_path);
        DefaultField(config.defaultSettings.threaded);

        Space();

        DefaultSettingsHeader(config.defaultSettings.hyperparameters);
        EditorGUI.indentLevel++;
        DefaultField(config.defaultSettings.hyperparameters.batch_size);
        DefaultField(config.defaultSettings.hyperparameters.buffer_size);
        DefaultField(config.defaultSettings.hyperparameters.learning_rate);
        DefaultField(config.defaultSettings.hyperparameters.learning_rate_schedule);

        Space();

        EditorGUILayout.LabelField("PPO Only", EditorStyles.miniBoldLabel);
        DefaultField(config.defaultSettings.hyperparameters.beta);
        DefaultField(config.defaultSettings.hyperparameters.epsilon);
        DefaultField(config.defaultSettings.hyperparameters.lambd);
        DefaultField(config.defaultSettings.hyperparameters.num_epoch);

        Space();

        EditorGUILayout.LabelField("SAC Only", EditorStyles.miniBoldLabel);
        DefaultField(config.defaultSettings.hyperparameters.init_entcoef);
        DefaultField(config.defaultSettings.hyperparameters.buffer_init_steps);
        DefaultField(config.defaultSettings.hyperparameters.tau);
        DefaultField(config.defaultSettings.hyperparameters.steps_per_update);
        DefaultField(config.defaultSettings.hyperparameters.reward_signal_num_update);
        DefaultField(config.defaultSettings.hyperparameters.save_replay_buffer);
        EditorGUI.indentLevel--;

        Space();

        DefaultSettingsHeader(config.defaultSettings.network_settings);
        EditorGUI.indentLevel++;
        DefaultField(config.defaultSettings.network_settings.hidden_units);
        DefaultField(config.defaultSettings.network_settings.num_layers);
        DefaultField(config.defaultSettings.network_settings.vis_encode_type);
        DefaultField(config.defaultSettings.network_settings.normalize);
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;

        Space();

        DefaultSettingsHeader(config.checkpointSettings);
        EditorGUI.indentLevel++;
        DefaultField(config.checkpointSettings.results_dir);
        DefaultField(config.checkpointSettings.run_id);
        DefaultField(config.checkpointSettings.initialize_from);
        DefaultField(config.checkpointSettings.resume);
        DefaultField(config.checkpointSettings.force);
        DefaultField(config.checkpointSettings.inference);
        DefaultField(config.checkpointSettings.load_model);
        DefaultField(config.checkpointSettings.train_model);
        EditorGUI.indentLevel--;

        Space();

        DefaultSettingsHeader(config.engineSettings);
        EditorGUI.indentLevel++;
        DefaultField(config.engineSettings.time_scale);
        DefaultField(config.engineSettings.quality_level);
        DefaultField(config.engineSettings.target_frame_rate);
        DefaultField(config.engineSettings.capture_frame_rate);
        DefaultField(config.engineSettings.width);
        DefaultField(config.engineSettings.height);
        DefaultField(config.engineSettings.no_graphics);
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

    public bool DefaultField(Config.Entry entry, bool isLabel = false)
    {
        var wrongTrainer = !entry.Trainers.Contains((string) config.defaultSettings.trainer_type.value);
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
                value = EditorGUILayout.TextField(entry.value);
                break;
            case nameof(System.Int32):
                value = EditorGUILayout.IntField(System.Int32.Parse(entry.value)).ToString();
                break;
            case nameof(System.Single):
                value = EditorGUILayout.FloatField(System.Single.Parse(entry.value)).ToString();
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