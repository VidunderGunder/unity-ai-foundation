#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public class ConfigEditor : EditorWindow
{
    static public string Title = "ML Agents Config";

    [MenuItem("ML Agents/Config")]
    public static void OpenWindow()
    {
        var w = GetWindow<ConfigEditor>(false, Title, true);
        w.minSize = new Vector2(300, 300);
    }
    [SerializeField]
    private bool newFileExists = false;
    [SerializeField]
    private string newFilename = "config";
    [SerializeField]
    private bool alwaysShowTooltips = false;
    [SerializeField]
    private int selectedIndexFile = 0;
    private string[] allExistingFile = new string[] { };
    private Config config = new Config();

    private void OnEnable()
    {
        var data = EditorPrefs.GetString("config", JsonUtility.ToJson(this, false));
        JsonUtility.FromJsonOverwrite(data, this);
        config.filename = newFilename;
        if (config.FileExists)
            config.ReadFile();
    }

    protected void OnDisable()
    {
        var data = JsonUtility.ToJson(this, false);
        EditorPrefs.SetString("config", data);
    }

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

    Vector2 scrollPosition = Vector2.zero;

    void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);
        EditorGUI.BeginDisabledGroup(Application.isPlaying);
        getAllExistingFile();
        // base.OnInspectorGUI();
        // Skin.Initialize();
        // serializedObject.Update();
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
            Space();
            AllFields();
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
        EditorGUI.EndDisabledGroup();
        GUILayout.EndScrollView();
    }

    private void NewFileFromDefault()
    {
        config.filename = newFilename;
        File.WriteAllText(
            config.Path,
            string.Empty
        );
        config.WriteFile();
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
        config.WriteFile();
    }

    // private void OnLostFocus()
    // {
    //     Debug.Log("Write");
    //     config.WriteFile();
    // }

    private void OpenFileButton()
    {
        if (GUILayout.Button("Open File")) EditorUtility.OpenWithDefaultApp(config.Path);
    }

    // public void OnSave()
    // {
    //     if (changed)
    //     {
    //         Undo.ClearUndo(config);
    //         config.WriteFile();
    //         changed = false;
    //     }
    // }
    private Dictionary<Dictionary<string, Config.Entry>, string> fieldNames = new Dictionary<Dictionary<string, Config.Entry>, string>() { };
    private Dictionary<Dictionary<string, Config.Entry>, string> values = new Dictionary<Dictionary<string, Config.Entry>, string>() { };

    private void DefaultFields(Dictionary<string, Config.Entry> entries, string trainer_type = null)
    {
        foreach (var entry in entries)
        {
            if (new[] { "ppo_only", "sac_only" }.Contains(entry.Key))
                EditorGUILayout.LabelField(entry.Value.ToString(), EditorStyles.miniBoldLabel);
            else
                DefaultField(entry.Value, entry.Key, trainer_type);
        }
        EditorGUILayout.LabelField("Custom field", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (fieldNames.ContainsKey(entries))
            fieldNames[entries] = EditorGUILayout.TextField("Label name", fieldNames[entries]);
        else fieldNames[entries] = EditorGUILayout.TextField("Label name", "");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (values.ContainsKey(entries))
            values[entries] = EditorGUILayout.TextField("Value", values[entries]);
        else values[entries] = EditorGUILayout.TextField("Value", "");
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(EditorGUI.indentLevel * 16);
        if (GUILayout.Button("Add custom"))
        {
            entries[fieldNames[entries]] = new Config.Entry(values[entries]);
            fieldNames[entries] = "";
            values[entries] = "";
            config.WriteFile();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void NetworkSettingsField(Config.TrainerSettings.NetworkSettings network_settings)
    {
        InUse(network_settings);
        if (network_settings.inUse)
        {
            DefaultSettingsHeader(network_settings);
            EditorGUI.indentLevel++;
            DefaultFields(network_settings.network_settings);
            InUse(network_settings.memory);
            if (network_settings.memory.inUse)
            {
                DefaultSettingsHeader(network_settings.memory);
                EditorGUI.indentLevel++;
                DefaultFields(network_settings.memory.memory);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
    }


    private void BehaviourFields(Config.TrainerSettings behaviour)
    {
        var trainer_type = behaviour?.trainer_settings?["trainer_type"]?.value;
        EditorGUI.indentLevel++;
        DefaultFields(behaviour.trainer_settings);

        Space();

        DefaultSettingsHeader(behaviour.hyperparameters);
        EditorGUI.indentLevel++;
        DefaultFields(behaviour.hyperparameters.hyperparameters);

        EditorGUI.indentLevel--;

        Space();

        NetworkSettingsField(behaviour.network_settings);

        InUse(behaviour.reward_signals);
        if (behaviour.reward_signals.inUse)
        {
            DefaultSettingsHeader(behaviour.reward_signals);
            EditorGUI.indentLevel++;
            DefaultFields(behaviour.reward_signals.reward_signals);
            InUse(behaviour.reward_signals.extrinsic);
            if (behaviour.reward_signals.extrinsic.inUse)
            {
                DefaultSettingsHeader(behaviour.reward_signals.extrinsic);
                EditorGUI.indentLevel++;
                DefaultFields(behaviour.reward_signals.extrinsic.extrinsic);
                EditorGUI.indentLevel--;
            }
            InUse(behaviour.reward_signals.curiosity);
            if (behaviour.reward_signals.curiosity.inUse)
            {
                DefaultSettingsHeader(behaviour.reward_signals.curiosity);
                EditorGUI.indentLevel++;
                DefaultFields(behaviour.reward_signals.curiosity.curiosity);
                NetworkSettingsField(behaviour.reward_signals.curiosity.network_settings);
                EditorGUI.indentLevel--;
            }
            InUse(behaviour.reward_signals.gail);
            if (behaviour.reward_signals.gail.inUse)
            {
                DefaultSettingsHeader(behaviour.reward_signals.gail);
                EditorGUI.indentLevel++;
                DefaultFields(behaviour.reward_signals.gail.gail);
                NetworkSettingsField(behaviour.reward_signals.gail.network_settings);
                EditorGUI.indentLevel--;
            }
            InUse(behaviour.reward_signals.rnd);
            if (behaviour.reward_signals.rnd.inUse)
            {
                DefaultSettingsHeader(behaviour.reward_signals.rnd);
                EditorGUI.indentLevel++;
                DefaultFields(behaviour.reward_signals.rnd.rnd);
                NetworkSettingsField(behaviour.reward_signals.rnd.network_settings);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }
        InUse(behaviour.behavior_cloning);
        if (behaviour.behavior_cloning.inUse)
        {
            DefaultSettingsHeader(behaviour.behavior_cloning);
            EditorGUI.indentLevel++;
            DefaultFields(behaviour.behavior_cloning.behavior_cloning);
            EditorGUI.indentLevel--;
        }
        InUse(behaviour.self_play);
        if (behaviour.self_play.inUse)
        {
            DefaultSettingsHeader(behaviour.self_play);
            EditorGUI.indentLevel++;
            DefaultFields(behaviour.self_play.self_play);
            EditorGUI.indentLevel--;
        }
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
            DefaultField(behaviors[i].behavior_name["behavior_name"], "behavior_name");
            BehaviourFields(behaviors[i].behaviour);
            EditorGUI.indentLevel--;
            if (GUILayout.Button("Remove Behaviour"))
            {
                config.multipleBehaviours.behaviors.RemoveAt(i);
                config.WriteFile();
            };
            Space();
        };
        if (GUILayout.Button("Add Behaviour"))
        {
            config.multipleBehaviours.behaviors.Add(new Config.TrainerSettingsMultipleBehaviour("behavior name " + (config.multipleBehaviours.behaviors.Count + 1)));
            config.WriteFile();
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
            DefaultFields(config.torchSettings.torch_settings);
            EditorGUI.indentLevel--;
            Space();
        }

        DefaultField(config.debug["debug"], "debug");
    }

    private void InUse(Config.Settings settings)
    {
        EditorGUI.BeginChangeCheck();
        settings.inUse = EditorGUILayout.Toggle("Use " + settings.Key.ToDirtyTitleCase(), settings.inUse);
        if (EditorGUI.EndChangeCheck())
            config.WriteFile();
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
            if (behavior.behavior_name["behavior_name"].value == value)
            {
                numberWithSameValue++;
            }
        }
        if (1 < numberWithSameValue)
        {
            EditorGUILayout.HelpBox("Behavior name are not unique", MessageType.Error);
        }

    }

    public bool DefaultField(Config.Entry entry, string key, string trainer_type = null)
    {
        var wrongTrainer = false;
        if (trainer_type != null)
        {
            wrongTrainer = !entry.Trainers.Contains((string) trainer_type);
        }
        var excludeEntry = !entry.AlwaysActive && !entry.active | wrongTrainer;

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(excludeEntry);
        var content = new GUIContent(entry.Label != null ? entry.Label.ToDirtyTitleCase() : key.ToDirtyTitleCase(), alwaysShowTooltips ? null : entry.Help);
        EditorGUIUtility.fieldWidth = 80;

        EditorGUI.BeginChangeCheck();
        string value = null;

        switch (entry.type.Name)
        {
            case nameof(System.String):
                if (entry.Path)
                {
                    EditorGUILayout.LabelField(key, GUILayout.Width(150));
                    if (GUILayout.Button("Selected file:" + entry.value.ToString(), GUILayout.MinWidth(10)))
                        value = EditorUtility.OpenFilePanel(key, entry.value, "*");
                }
                else if (entry.Options == null || entry.Options.Length == 0) value = EditorGUILayout.TextField(content, entry.value);
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
        switch (key)
        {
            case "behavior_name":
                uniqueBehaviorName(entry.value);
                break;
            default:
                break;
        }

        if (EditorGUI.EndChangeCheck())
        {
            // Undo.RecordObject(config, "Changed Trainer Config");
            if (value != null)
            {
                entry.value = value;
                config.WriteFile();
            };
            if (active != entry.active)
            {
                entry.active = active;
                config.WriteFile();
            };
        }

        if (entry.Help != null && entry.Help != "" && alwaysShowTooltips)
        {
            EditorGUILayout.HelpBox(entry.Help, MessageType.None);
            EditorGUILayout.Space(5);
        }

        return !excludeEntry;
    }
    public class Config
    {
        public static List<string> defaultTrainers = new List<string> { "ppo", "sac", "poca" };

        [HideInInspector]
        public string filename;
        [HideInInspector]
        public string Path => GetCWD() + "/" + filename + ".yaml";
        [HideInInspector]
        public bool FileExists => File.Exists(Path);

        [HideInInspector]
        public string GetCWD(bool full = false)
        {
            string scriptPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName().Replace('\\', '/');
            string assetPath = Application.dataPath;

            string path;
            string[] pathArray;

            if (full)
            {
                path = scriptPath;
                pathArray = path.Split('/');
            }
            else
            {
                path = scriptPath.Replace(assetPath, "Assets");
                pathArray = path.Split('/');
            }

            string currentFolderProjectPath = System.String.Join("/", pathArray.Take(pathArray.Length - 1));
            return currentFolderProjectPath;
        }
        // ReadFile
        [HideInInspector]
        Dictionary<string, Dictionary<string, string>> readData = new Dictionary<string, Dictionary<string, string>>();
        [HideInInspector]
        Dictionary<string, Dictionary<string, Dictionary<string, string>>> readDataMultipleBehaviours = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        // YAML
        [HideInInspector]
        public TrainerSettings defaultSettings = new TrainerSettings("default_settings", alwaysActive: true);
        [HideInInspector]
        public MultipleBehaviours multipleBehaviours = new MultipleBehaviours("behaviors", alwaysActive: false);
        [HideInInspector]
        public EngineSettings engineSettings = new EngineSettings();
        [HideInInspector]
        public EnvironmentParameters environmentParameters = new EnvironmentParameters();
        [HideInInspector]
        public CheckpointSettings checkpointSettings = new CheckpointSettings();
        [HideInInspector]
        public TorchSettings torchSettings = new TorchSettings();
        [HideInInspector]
        public Dictionary<string, Entry> debug = new Dictionary<string, Entry>() { { "debug", new Entry(false) } };


        #region Settings
        [System.Serializable]
        public class TrainerSettings : Settings
        {
            private static bool? active = null;
            public TrainerSettings(string key, bool alwaysActive, bool inUse = true, bool? newActive = null) : base(key)
            {
                active = newActive;
            }

            [SerializeField]
            public Dictionary<string, Entry> trainer_settings = new Dictionary<string, Entry>() {
               { "trainer_type", new Entry("ppo",
                alwaysActive: true,
                help: "(default = ppo) The type of trainer to use: ppo, sac, or poca.",
                options: new string[] { "ppo", "sac", "poca" }
                    )},
                {"summary_freq", new Entry(
                    50000,
                active:  active,
                help:
                    "(default = 50000) Number of experiences that needs to be collected before generating and displaying training statistics. This determines the granularity of the graphs in Tensorboard."
                )},
                {"time_horizon", new Entry(
                    64,
                    active: active,
                    range: new System.Tuple<int, int>(32, 2048),
                    help:
                    "(default = 64) How many steps of experience to collect per-agent before adding it to the experience buffer. When this limit is reached before the end of an episode, a value estimate is used to predict the overall expected reward from the agent's current state. As such, this parameter trades off between a less biased, but higher variance estimate (long time horizon) and more biased, but less varied estimate (short time horizon). In cases where there are frequent rewards within an episode, or episodes are prohibitively large, a smaller number can be more ideal. This number should be large enough to capture all the important behavior within a sequence of an agent's actions.\n\nTypical range: 32 - 2048"
                )},
                {"max_steps", new Entry(
                    500000,
                    active: active,
                    range: new System.Tuple<int, int>(500000, 10000000),
                    help:
                    "(default = 500000) Total number of steps (i.e., observation collected and action taken) that must be taken in the environment (or across all environments if using multiple in parallel) before ending the training process. If you have multiple agents with the same behavior name within your environment, all steps taken by those agents will contribute to the same max_steps count.\n\nTypical range: 5e5 - 1e7"
                )},
                {"keep_checkpoints", new Entry(
                    5,
                    active: active,
                    help:
                    "(default = 5) The maximum number of model checkpoints to keep. Checkpoints are saved after the number of steps specified by the checkpoint_interval option. Once the maximum number of checkpoints has been reached, the oldest checkpoint is deleted when saving a new checkpoint."
                )},
                {"checkpoint_interval",new Entry(
                    500000,
                    active: active,
                    help:
                        "(default = 500000) The number of experiences collected between each checkpoint by the trainer. A maximum of keep_checkpoints checkpoints are saved before old ones are deleted. Each checkpoint saves the .onnx files in results/ folder."
                )},
                {"init_path", new Entry(
                    "",
                active: active,
                help:
                    "(default = None) Initialize trainer from a previously saved model. Note that the prior run should have used the same trainer configurations as the current run, and have been saved with the same version of ML-Agents. You should provide the full path to the folder where the checkpoints were saved, e.g. ./models/{run-id}/{behavior_name}. This option is provided in case you want to initialize different behaviors from different runs; in most cases, it is sufficient to use the --initialize-from CLI parameter to initialize all models from the same run."
                )},
                {"threaded", new Entry(
                    true,
                active: active,
                help:
                    "(default = true) By default, model updates can happen while the environment is being stepped. This violates the on-policy assumption of PPO slightly in exchange for a training speedup. To maintain the strict on-policyness of PPO, you can disable parallel updates by setting threaded to false. There is usually no reason to turn threaded off for SAC."
                )}};

            public NetworkSettings network_settings = new NetworkSettings(inUse: true, newActive: active);
            public Hyperparameters hyperparameters = new Hyperparameters(newActive: active);
            public RewardSignals reward_signals = new RewardSignals(newActive: active);
            public BehavioralCloning behavior_cloning = new BehavioralCloning(newActive: active);
            public SelfPlay self_play = new SelfPlay(newActive: active);

            [System.Serializable]
            public class Hyperparameters : Settings
            {
                private static bool? active = null;
                public Hyperparameters(string key = "hyperparameters", bool? newActive = null) : base(key)
                {
                    active = newActive;
                }
                public Dictionary<string, Entry> hyperparameters = new Dictionary<string, Entry>()
            {
                 {"learning_rate", new Entry(
                    0.0003f,
                    active: active,
                    range: new System.Tuple<float, float>(0.001f, 0.00001f),
                    help:
                    "(default = 3e-4) Initial learning rate for gradient descent. Corresponds to the strength of each gradient descent update step. This should typically be decreased if training is unstable, and the reward does not consistently increase.\n\nTypical range: 1e-5 - 1e-3"
                )},
                 {"batch_size", new Entry(
                    256,
                    active: active,
                    help:
                    "Number of experiences in each iteration of gradient descent. This should always be multiple times smaller than buffer_size. If you are using continuous actions, this value should be large (on the order of 1000s). If you are using only discrete actions, this value should be smaller (on the order of 10s).\n\nTypical range: (Continuous - PPO): 512 - 5120; (Continuous - SAC): 128 - 1024; (Discrete, PPO & SAC): 32 - 512."
                )},
                 {"buffer_size", new Entry(
                    10240,
                    active: active,
                    help:
                    "(default = 10240 for PPO and 50000 for SAC)\n\nPPO: Number of experiences to collect before updating the policy model. Corresponds to how many experiences should be collected before we do any learning or updating of the model. This should be multiple times larger than batch_size. Typically a larger buffer_size corresponds to more stable training updates.\n\nSAC: The max size of the experience buffer - on the order of thousands of times longer than your episodes, so that SAC can learn from old as well as new experiences."
                )},
                 {"learning_rate_schedule", new Entry(
                    "linear",
                    active: active,
                    help:
                    "(default = linear for PPO and constant for SAC) Determines how learning rate changes over time. For PPO, we recommend decaying learning rate until max_steps so learning converges more stably. However, for some cases (e.g. training for an unknown amount of time) this feature can be disabled. For SAC, we recommend holding learning rate constant so that the agent can continue to learn until its Q function converges naturally.\n\nlinear decays the learning_rate linearly, reaching 0 at max_steps, while constant keeps the learning rate constant for the entire training run."
                )},
                {"beta", new Entry(
                    0.005f,
                    active: active,
                    range: new System.Tuple<float, float>(0.01f, 0.0001f),
                    trainers: new List<string> { "ppo", "poca" },
                    help:
                    "(default = 5.0e-3) Strength of the entropy regularization, which makes the policy \"more random.\" This ensures that agents properly explore the action space during training. Increasing this will ensure more random actions are taken. This should be adjusted such that the entropy (measurable from TensorBoard) slowly decreases alongside increases in reward. If entropy drops too quickly, increase beta. If entropy drops too slowly, decrease beta.\n\nTypical range: 1e-4 - 1e-2"
                )},
                {"epsilon", new Entry(
                    0.2f,
                    active: active,
                    trainers: new List<string> { "ppo", "poca" },
                    range: new System.Tuple<float, float>(0.1f, 0.3f),
                    help:
                    "(default = 0.2) Influences how rapidly the policy can evolve during training. Corresponds to the acceptable threshold of divergence between the old and new policies during gradient descent updating. Setting this value small will result in more stable updates, but will also slow the training process.\n\nTypical range: 0.1 - 0.3"
                )},
                {
                "lambd", new Entry( // Not sure if `lambd` is a typo?
                    0.95f,
                    active: active,
                    range: new System.Tuple<float, float>(0.9f, 0.95f),
                    trainers: new List<string> { "ppo", "poca" },
                    help:
                    "(default = 0.95) Regularization parameter (lambda) used when calculating the Generalized Advantage Estimate (GAE). This can be thought of as how much the agent relies on its current value estimate when calculating an updated value estimate. Low values correspond to relying more on the current value estimate (which can be high bias), and high values correspond to relying more on the actual rewards received in the environment (which can be high variance). The parameter provides a trade-off between the two, and the right value can lead to a more stable training process.\n\nTypical range: 0.9 - 0.95"
                )},
                            {
                "num_epoch", new Entry(
                    3,
                    active: active,
                    trainers: new List<string> { "ppo", "poca" },
                    range: new System.Tuple<int, int>(3, 10),
                    help:
                    "(default = 3) Number of passes to make through the experience buffer when performing gradient descent optimization.The larger the batch_size, the larger it is acceptable to make this. Decreasing this will ensure more stable updates, at the cost of slower learning.\n\nTypical range: 1000 - 10000"
                )},
                            {
                "buffer_init_steps", new Entry(
                    0,
                    active: active,
                    trainers: new List<string> { "sac" },
                    range: new System.Tuple<int, int>(1000, 10000),
                    help:
                    "(default = 0) Number of experiences to collect into the buffer before updating the policy model. As the untrained policy is fairly random, pre-filling the buffer with random actions is useful for exploration. Typically, at least several episodes of experiences should be pre-filled.\n\nTypical range: 1000 - 10000"
                )},
                            {
                "init_entcoef", new Entry(
                    1.0f,
                    active: active,
                    trainers: new List<string> { "sac" },
                    range: new System.Tuple<float, float>(0.05f, 1f),
                    help:
                    "(default = 1.0) How much the agent should explore in the beginning of training. Corresponds to the initial entropy coefficient set at the beginning of training. In SAC, the agent is incentivized to make its actions entropic to facilitate better exploration. The entropy coefficient weighs the true reward with a bonus entropy reward. The entropy coefficient is automatically adjusted to a preset target entropy, so the init_entcoef only corresponds to the starting value of the entropy bonus. Increase init_entcoef to explore more in the beginning, decrease to converge to a solution faster.\n\nTypical range: (Continuous): 0.5 - 1.0; (Discrete): 0.05 - 0.5"
                )},
                            {
                "save_replay_buffer", new Entry(
                    false,
                    active: active,
                    trainers: new List<string> { "sac" },
                    help:
                    "(default = false) Whether to save and load the experience replay buffer as well as the model when quitting and re-starting training. This may help resumes go more smoothly, as the experiences collected won't be wiped. Note that replay buffers can be very large, and will take up a considerable amount of disk space. For that reason, we disable this feature by default."
                )},
                            {
                "tau", new Entry(
                    0.005f,
                    active: active,
                    trainers: new List<string> { "sac" },
                    range: new System.Tuple<float, float>(0.05f, 0.01f),
                    help:
                    "(default = 0.005) How aggressively to update the target network used for bootstrapping value estimation in SAC. Corresponds to the magnitude of the target Q update during the SAC model update. In SAC, there are two neural networks: the target and the policy. The target network is used to bootstrap the policy's estimate of the future rewards at a given state, and is fixed while the policy is being updated. This target is then slowly updated according to tau. Typically, this value should be left at 0.005. For simple problems, increasing tau to 0.01 might reduce the time it takes to learn, at the cost of stability.\n\nTypical range: 0.005 - 0.01"
                )},
                            {
                "steps_per_update", new Entry(
                    1,
                    active: active,
                    trainers: new List<string> { "sac" },
                    range: new System.Tuple<int, int>(1, 20),
                    help:
                    "(default = 1) Average ratio of agent steps (actions) taken to updates made of the agent's policy. In SAC, a single \"update\" corresponds to grabbing a batch of size batch_size from the experience replay buffer, and using this mini batch to update the models. Note that it is not guaranteed that after exactly steps_per_update steps an update will be made, only that the ratio will hold true over many steps. Typically, steps_per_update should be greater than or equal to 1. Note that setting steps_per_update lower will improve sample efficiency (reduce the number of steps required to train) but increase the CPU time spent performing updates. For most environments where steps are fairly fast (e.g. our example environments) steps_per_update equal to the number of agents in the scene is a good balance. For slow environments (steps take 0.1 seconds or more) reducing steps_per_update may improve training speed. We can also change steps_per_update to lower than 1 to update more often than once per step, though this will usually result in a slowdown unless the environment is very slow.\n\nTypical range: 1 - 20"
                )},
                            {
                "reward_signal_num_update", new Entry(
                    "steps_per_update",
                    active: active,
                    trainers: new List<string> { "sac" },
                    help:
                    "(default = steps_per_update) Number of steps per mini batch sampled and used for updating the reward signals. By default, we update the reward signals once every time the main policy is updated. However, to imitate the training procedure in certain imitation learning papers (e.g. Kostrikov et. al, Blondé et. al), we may want to update the reward signal (GAIL) M times for every update of the policy. We can change steps_per_update of SAC to N, as well as reward_signal_steps_per_update under reward_signals to N / M to accomplish this. By default, reward_signal_steps_per_update is set to steps_per_update."
                )}
                };
            };
            [System.Serializable]
            public class RewardSignals : Settings
            {
                private static bool? active = null;
                public RewardSignals(string key = "reward_signals", bool inUse = false, bool? newActive = null) : base(key, inUse: inUse)
                {
                    active = newActive;
                }
                public Dictionary<string, Entry> reward_signals = new Dictionary<string, Entry>()
                {
                {"strength", new Entry(
                    1.0f,
                    active: active,
                    range: new System.Tuple<float, float>(0.001f, 0.01f),
                    help:
                    "	(default = 1.0) Magnitude of the curiosity reward generated by the intrinsic rnd module. This should be scaled in order to ensure it is large enough to not be overwhelmed by extrinsic reward signals in the environment. Likewise it should not be too large to overwhelm the extrinsic reward signal."
                )},
                {"num_layers", new Entry(
                    0.99f,
                    active: active,
                    range: new System.Tuple<float, float>(0.8f, 0.995f),
                    help: "(default = 0.99) Discount factor for future rewards."
                )},
                {"learning_rate", new Entry(
                    0.0003f,
                    active: active,
                    range: new System.Tuple<float, float>(0.003f, 0.00003f),
                    help:
                    "(default = 3e-4) Learning rate used to update the RND module. This should be large enough for the RND module to quickly learn the state representation, but small enough to allow for stable learning."
                )},
                };
                public NetworkSettings network_settings = new NetworkSettings(newActive: active);
                public Extrinsic extrinsic = new Extrinsic(newActive: active);
                public Curiosity curiosity = new Curiosity(newActive: active);
                public Gail gail = new Gail(newActive: active);
                public Rnd rnd = new Rnd(newActive: active);
                [System.Serializable]
                public class Extrinsic : Settings
                {
                    private static bool? active = null;
                    public Extrinsic(string key = "extrinsic", bool inUse = false, bool? newActive = null) : base(key, inUse: inUse)
                    {
                        active = newActive;
                    }
                    public Dictionary<string, Entry> extrinsic = new Dictionary<string, Entry>()
                    {
                    {"strength", new Entry(
                        1.0f,
                        active: active,
                        help:
                        "(default = 1.0) Factor by which to multiply the reward given by the environment. Typical ranges will vary depending on the reward signal."
                    )},
                    {"gamma", new Entry(
                        0.99f,
                        active: active,
                        range: new System.Tuple<float, float>(0.8f, 0.995f),
                        help:
                        "(default = 0.99) Discount factor for future rewards coming from the environment. This can be thought of as how far into the future the agent should care about possible rewards. In situations when the agent should be acting in the present in order to prepare for rewards in the distant future, this value should be large. In cases when rewards are more immediate, it can be smaller. Must be strictly smaller than 1."
                    )},
                    };
                }
                [System.Serializable]
                public class Curiosity : Settings
                {
                    private static bool? active = null;
                    public Curiosity(string key = "curiosity", bool inUse = false, bool? newActive = null) : base(key, inUse: inUse)
                    { active = newActive; }
                    public Dictionary<string, Entry> curiosity = new Dictionary<string, Entry>()
                    {
                    {"strength", new Entry(
                        1.0f,
                        active: active,
                        range: new System.Tuple<float, float>(0.1f, 0.001f),
                        help:
                        "(default = 1.0) Magnitude of the curiosity reward generated by the intrinsic curiosity module. This should be scaled in order to ensure it is large enough to not be overwhelmed by extrinsic reward signals in the environment. Likewise it should not be too large to overwhelm the extrinsic reward signal."
                    )},
                    {"gamma", new Entry(
                        0.99f,
                        active: active,
                        range: new System.Tuple<float, float>(0.8f, 0.995f),
                        help:
                        "(default = 0.99) Discount factor for future rewards."
                    )},
                    {"learning_rate", new Entry(
                        0.0003f,
                        active: active,
                        range: new System.Tuple<float, float>(0.003f, 0.00003f),
                        help:
                        "(default = 3e-4) Learning rate used to update the intrinsic curiosity module. This should typically be decreased if training is unstable, and the curiosity loss is unstable."
                    )},
                    };
                    public NetworkSettings network_settings = new NetworkSettings(newActive: active);
                }
                [System.Serializable]
                public class Gail : Settings
                {
                    private static bool? active = null;
                    public Gail(string key = "gail", bool inUse = false, bool? newActive = null) : base(key, inUse: inUse) { active = newActive; }
                    public Dictionary<string, Entry> gail = new Dictionary<string, Entry>()
                    {
                    {"strength", new Entry(
                        1.0f,
                        active: active,
                        range: new System.Tuple<float, float>(1.0f, 0.01f),
                        help:
                        "(default = 1.0) Factor by which to multiply the raw reward. Note that when using GAIL with an Extrinsic Signal, this value should be set lower if your demonstrations are suboptimal (e.g. from a human), so that a trained agent will focus on receiving extrinsic rewards instead of exactly copying the demonstrations. Keep the strength below about 0.1 in those cases."
                    )},
                    {"gamma", new Entry(
                        0.99f,
                        active: active,
                        range: new System.Tuple<float, float>(0.8f, 0.9f),
                        help:
                        "(default = 0.99) Discount factor for future rewards."
                    )},
                    {"demo_path", new Entry(
                        "",
                        path: true,
                        active: active,
                        help: "(Required, no default) The path to your .demo file or directory of .demo files."
                    )},
                    {"learning_rate", new Entry(
                        0.0003f,
                        range: new System.Tuple<float, float>(0.003f, 0.00003f),
                        active: active,
                        help: "(Optional, default = 3e-4) Learning rate used to update the discriminator. This should typically be decreased if training is unstable, and the GAIL loss is unstable."
                    )},
                    {"use_actions", new Entry(
                        false,
                        active: active,
                        help:
                        "(default = false) Determines whether the discriminator should discriminate based on both observations and actions, or just observations. Set to True if you want the agent to mimic the actions from the demonstrations, and False if you'd rather have the agent visit the same states as in the demonstrations but with possibly different actions. Setting to False is more likely to be stable, especially with imperfect demonstrations, but may learn slower."
                    )},
                    {"use_vail", new Entry(
                        false,
                        active: active,
                        help:
                        "(default = false) Enables a variational bottleneck within the GAIL discriminator. This forces the discriminator to learn a more general representation and reduces its tendency to be 'too good' at discriminating, making learning more stable. However, it does increase training time. Enable this if you notice your imitation learning is unstable, or unable to learn the task at hand."
                    )},
                    };
                    public NetworkSettings network_settings = new NetworkSettings(newActive: active);
                }
                [System.Serializable]
                public class Rnd : Settings
                {
                    private static bool? active = null;
                    public Rnd(string key = "rnd", bool inUse = false, bool? newActive = null) : base(key, inUse: inUse) { active = newActive; }
                    public Dictionary<string, Entry> rnd = new Dictionary<string, Entry>()
                    {
                    {"strength", new Entry(
                        1.0f,
                        active: active,
                        range: new System.Tuple<float, float>(0.00f, 0.01f),
                        help:
                        "(default = 1.0) Magnitude of the curiosity reward generated by the intrinsic rnd module. This should be scaled in order to ensure it is large enough to not be overwhelmed by extrinsic reward signals in the environment. Likewise it should not be too large to overwhelm the extrinsic reward signal."
                    )},
                    {"gamma", new Entry(
                        0.99f,
                        active: active,
                        range: new System.Tuple<float, float>(0.8f, 0.995f),
                        help:
                        "(default = 0.99) Discount factor for future rewards."
                    )},
                    {"learning_rate", new Entry(
                        0.0003f,
                        range: new System.Tuple<float, float>(0.003f, 0.00003f),
                        active: active,
                        help: "(Optional, default = 3e-4) Learning rate used to update the discriminator. This should typically be decreased if training is unstable, and the GAIL loss is unstable."
                    )},
                    };
                    public NetworkSettings network_settings = new NetworkSettings(inUse: false, newActive: active);

                }
            }
            [System.Serializable]
            public class BehavioralCloning : Settings
            {
                private static bool? active = null;
                public BehavioralCloning(string key = "behavior_cloning", bool inUse = false, bool? newActive = null) : base(key, inUse: inUse) { active = newActive; }
                public Dictionary<string, Entry> behavior_cloning = new Dictionary<string, Entry>()
                {
                    {"demo_path", new Entry(
                        "",
                        active: active,
                        path: true,
                        help:
                        "(Required, no default) The path to your .demo file or directory of .demo files."
                    )},
                    {"strength", new Entry(
                        1.0f,
                        active: active,
                        range: new System.Tuple<float, float>(0.1f, 0.5f),
                        help:
                        "(default = 1.0) Learning rate of the imitation relative to the learning rate of PPO, and roughly corresponds to how strongly we allow BC to influence the policy."
                    )},
                    {"steps", new Entry(
                         0,
                        active: active,
                        help:
                        "(default = 0) During BC, it is often desirable to stop using demonstrations after the agent has 'seen' rewards, and allow it to optimize past the available demonstrations and/or generalize outside of the provided demonstrations. steps corresponds to the training steps over which BC is active. The learning rate of BC will anneal over the steps. Set the steps to 0 for constant imitation over the entire training run."
                    )},
                    {"batch_size", new Entry(
                         512,
                        active: false,
                        help:
                        "(default = batch_size of trainer) Number of demonstration experiences used for one iteration of a gradient descent update. If not specified, it will default to the batch_size of the trainer. Typical range: (Continuous): 512 - 5120; (Discrete): 32 - 512"
                    )},
                    {"num_epoch", new Entry(
                         3,
                        active: active,
                        range: new System.Tuple<float, float>(3, 10),
                        help:
                        "(default = num_epoch of trainer) Number of passes through the experience buffer during gradient descent. If not specified, it will default to the number of epochs set for PPO."
                    )},
                    {"samples_per_update", new Entry(
                         0,
                        active: active,
                        help:
                        "(default = 0) Maximum number of samples to use during each imitation update. You may want to lower this if your demonstration dataset is very large to avoid overfitting the policy on demonstrations. Set to 0 to train over all of the demonstrations at each update step. Typical range: buffer_size"
                    )},
                };
            }
            [System.Serializable]
            public class SelfPlay : Settings
            {
                private static bool? active = null;
                public SelfPlay(string key = "self_play", bool inUse = false, bool? newActive = null) : base(key, inUse: inUse) { active = newActive; }
                public Dictionary<string, Entry> self_play = new Dictionary<string, Entry>()
                {
                     {"save_steps",new Entry(
                         20000,
                        range: new System.Tuple<int, int>(10000, 100000),
                        active: active,
                        help:
                        "(default = 20000) Number of trainer steps between snapshots. For example, if save_steps=10000 then a snapshot of the current policy will be saved every 10000 trainer steps. Note, trainer steps are counted per agent. For more information, please see the migration doc after v0.13. A larger value of save_steps will yield a set of opponents that cover a wider range of skill levels and possibly play styles since the policy receives more training. As a result, the agent trains against a wider variety of opponents. Learning a policy to defeat more diverse opponents is a harder problem and so may require more overall training steps but also may lead to more general and robust policy at the end of training. This value is also dependent on how intrinsically difficult the environment is for the agent."
                    )},
                     {"team_change",new Entry(
                         100000,
                        active: active,
                        help:
                        "(default = 5 * save_steps) Number of trainer_steps between switching the learning team. This is the number of trainer steps the teams associated with a specific ghost trainer will train before a different team becomes the new learning team. It is possible that, in asymmetric games, opposing teams require fewer trainer steps to make similar performance gains. This enables users to train a more complicated team of agents for more trainer steps than a simpler team of agents per team switch. A larger value of team-change will allow the agent to train longer against it's opponents. The longer an agent trains against the same set of opponents the more able it will be to defeat them. However, training against them for too long may result in overfitting to the particular opponent strategies and so the agent may fail against the next batch of opponents. The value of team-change will determine how many snapshots of the agent's policy are saved to be used as opponents for the other team. So, we recommend setting this value as a function of the save_steps parameter discussed previously. Typical range: 4x-10x where x=save_steps"
                    )},
                     {"swap_steps",new Entry(
                         10000,
                        range: new System.Tuple<int, int>(10000, 100000),
                        active: active,
                        help:
                        "(default = 10000) Number of ghost steps (not trainer steps) between swapping the opponents policy with a different snapshot. A 'ghost step' refers to a step taken by an agent that is following a fixed policy and not learning. The reason for this distinction is that in asymmetric games, we may have teams with an unequal number of agents e.g. a 2v1 scenario like our Strikers Vs Goalie example environment. The team with two agents collects twice as many agent steps per environment step as the team with one agent. Thus, these two values will need to be distinct to ensure that the same number of trainer steps corresponds to the same number of opponent swaps for each team. The formula for swap_steps if a user desires x swaps of a team with num_agents agents against an opponent team with num_opponent_agents agents during team-change total steps is: (num_agents / num_opponent_agents) * (team_change / x) Typical range: 10000 - 100000"
                    )},
                     {"play_against_latest_model_ratio",new Entry(
                         0.0f,
                        active: false,
                        range: new System.Tuple<float, float>(0.0f, 0.1f),
                        help:
                        "A larger value of play_against_latest_model_ratio indicates that an agent will be playing against the current opponent more often. Since the agent is updating it's policy, the opponent will be different from iteration to iteration. This can lead to an unstable learning environment, but poses the agent with an auto-curricula of more increasingly challenging situations which may lead to a stronger final policy. Typical range: 0.0 - 1.0"
                    )},
                     {"num_epoch",new Entry(
                         3,
                        active: active,
                        range: new System.Tuple<float, float>(3, 10),
                        help:
                        "(default = num_epoch of trainer) Number of passes through the experience buffer during gradient descent. If not specified, it will default to the number of epochs set for PPO."
                    )},
                     {"samples_per_update",new Entry(
                         10,
                        active: active,
                        range: new System.Tuple<float, float>(5, 30),
                        help:
                        "(default = 10) Size of the sliding window of past snapshots from which the agent's opponents are sampled. For example, a window size of 5 will save the last 5 snapshots taken. Each time a new snapshot is taken, the oldest is discarded. A larger value of window means that an agent's pool of opponents will contain a larger diversity of behaviors since it will contain policies from earlier in the training run. Like in the save_steps hyperparameter, the agent trains against a wider variety of opponents. Learning a policy to defeat more diverse opponents is a harder problem and so may require more overall training steps but also may lead to more general and robust policy at the end of training. Typical range: 5 - 30"
                    )},
                };
            }
            [System.Serializable]
            public class NetworkSettings : Settings
            {
                private static bool? active = null;
                public NetworkSettings(string key = "network_settings", bool inUse = false, bool? newActive = null) : base(key, inUse: inUse) { active = newActive; }
                public Memory memory = new Memory(newActive: active);
                public Dictionary<string, Entry> network_settings = new Dictionary<string, Entry>()
            {
             {  "hidden_units",new Entry(
               128,
                active: active,
                range: new System.Tuple<int, int>(32, 512),
                help:
                "(default = 128) Number of units in the hidden layers of the neural network. Correspond to how many units are in each fully connected layer of the neural network. For simple problems where the correct action is a straightforward combination of the observation inputs, this should be small. For problems where the action is a very complex interaction between the observation variables, this should be larger.\n\nTypical range: 32 - 512"
            )},
             {  "num_layers",new Entry(
               2,
                active: active,
                range: new System.Tuple<int, int>(1, 3),
                help:
                "(default = 2) The number of hidden layers in the neural network. Corresponds to how many hidden layers are present after the observation input, or after the CNN encoding of the visual observation. For simple problems, fewer layers are likely to train faster and more efficiently. More layers may be necessary for more complex control problems.\n\nTypical range: 1 - 3"
            )},
             {  "normalize",new Entry(
               false,
                active: active,
                help:
                "(default = false) Whether normalization is applied to the vector observation inputs. This normalization is based on the running average and variance of the vector observation. Normalization can be helpful in cases with complex continuous control problems, but may be harmful with simpler discrete control problems."
            )},
             {  "vis_encode_type",new Entry(
               "simple",
                active: active,
                help:
                "(default = simple) Encoder type for encoding visual observations.\n\nsimple (default) uses a simple encoder which consists of two convolutional layers, nature_cnn uses the CNN implementation proposed by Mnih et al., consisting of three convolutional layers, and resnet uses the IMPALA Resnet consisting of three stacked layers, each with two residual blocks, making a much larger network than the other two. match3 is a smaller CNN (Gudmundsoon et al.) that is optimized for board games, and can be used down to visual observation sizes of 5x5."
            )},
            };

                [System.Serializable]
                public class Memory : Settings
                {
                    private static bool? active = null;
                    public Memory(string key = "memory", bool inUse = false, bool? newActive = null) : base(key, inUse: inUse) { active = newActive; }
                    public Dictionary<string, Entry> memory = new Dictionary<string, Entry>()
            {
            {"memory_size", new Entry(
                128,
                active: active,
                range: new System.Tuple<int, int>(32, 256),
                help:
                "(default = 128) Size of the memory an agent must keep. In order to use a LSTM, training requires a sequence of experiences instead of single experiences. Corresponds to the size of the array of floating point numbers used to store the hidden state of the recurrent neural network of the policy. This value must be a multiple of 2, and should scale with the amount of information you expect the agent will need to remember in order to successfully complete the task."
            )},
            {"sequence_length", new Entry(
                64,
                active: active,
                range: new System.Tuple<int, int>(4, 128),
                help:
                "(default = 64) Defines how long the sequences of experiences must be while training. Note that if this number is too small, the agent will not be able to remember things over longer periods of time. If this number is too large, the neural network will take longer to train."
            )}
            };
                }
            }
        }

        [System.Serializable]
        public class TrainerSettingsMultipleBehaviour
        {
            private static bool? active = null;
            public TrainerSettingsMultipleBehaviour(string behaviorName, bool? newActive = null)
            {
                this.behavior_name.Add(
                    "behavior_name", new Entry(
                                                behaviorName,
                                                alwaysActive: true,
                                                help: "Name the behavior have to be unique"
                                            ));
                active = newActive;
            }
            public Dictionary<string, Entry> behavior_name = new Dictionary<string, Entry>();
            public TrainerSettings behaviour = new TrainerSettings("default_settings", alwaysActive: true, newActive: active);
        };
        [System.Serializable]
        public class MultipleBehaviours : Settings
        {
            public MultipleBehaviours(string key, bool alwaysActive) : base(key) { }
            public List<TrainerSettingsMultipleBehaviour> behaviors = new List<TrainerSettingsMultipleBehaviour>();
        };


        [System.Serializable]
        public class EngineSettings : Settings
        {
            private static bool? active = null;
            public EngineSettings(string key = "engine_settings", bool inUse = true, bool? newActive = null) : base(key) { active = newActive; }
            public Dictionary<string, Entry> engine_settings = new Dictionary<string, Entry>() {
                {"time_scale",          new Entry(1.0f, active: active)},
                {"quality_level",          new Entry(5, active: active)},
                {"target_frame_rate", new Entry(-1, active: active)},
                {"capture_frame_rate",          new Entry(60, active: active)},
                {"no_graphics",          new Entry(false, active: active)},
                {"width",          new Entry(84, active: active)},
                {"height",          new Entry(84, active: active)}
            };
        }
        [System.Serializable]
        public class EnvironmentParameters : Settings
        {
            public EnvironmentParameters(string key = "environment_parameters", bool inUse = true) : base(key) { }
        }
        [System.Serializable]
        public class CheckpointSettings : Settings
        {
            private static bool? active = null;
            public CheckpointSettings(string key = "checkpoint_settings", bool inUse = true, bool? newActive = null) : base(key) { active = newActive; }
            public Dictionary<string, Entry> checkpoint_settings = new Dictionary<string, Entry>() {
                {"run_id", new Entry("mlady", active: active)},
                {"initialize_from", new Entry("", active: active)},
                {"load_model",          new Entry(false, active: active)},
                {"resume",          new Entry(false, active: active)},
                {"force",          new Entry(false, active: active)},
                {"train_model",          new Entry(false, active: active)},
                {"inference",          new Entry(false, active: active)},
                {"results_dir", new Entry("Results", active: active)}
            };
        }
        [System.Serializable]
        public class TorchSettings : Settings
        {
            private static bool? active = null;
            public TorchSettings(string key = "torch_settings", bool inUse = true, bool? newActive = null) : base(key) { active = newActive; }
            public Dictionary<string, Entry> torch_settings = new Dictionary<string, Entry>() { { "device", new Entry("cuda", active: active) } };
        }
        #endregion

        public interface IYAMLLine
        {
            string Label { get; }
            string Help { get; }
            bool Active { get; }
            bool AlwaysActive { get; }
        }

        public interface IYAMLEntry : IYAMLLine
        {
            object Value { get; }
        }

        [System.Serializable]
        public class Settings : IYAMLLine
        {

            public bool inUse;
            private string key;
            private string label;
            private string help;
            private bool active;
            private bool alwaysActive;

            public string Key => key;
            public string Label => label;
            public string Help => help;
            public bool Active => active;
            public bool AlwaysActive => alwaysActive;

            public Settings(
                string key,
                string label = null,
                string help = null,
                bool active = true,
                bool alwaysActive = false,
                bool inUse = true
            )
            {
                this.key = key;
                this.label = label != null ? label : key.ToDirtyTitleCase();
                this.help = help;
                this.active = alwaysActive || active;
                this.alwaysActive = alwaysActive;
                this.inUse = inUse;
            }
        }

        [System.Serializable]
        public class Entry : IYAMLLine
        {
            public bool active;
            public string value;
            public System.Type type;
            public int selectedIndex = 0;

            private string help;
            private string label;
            private List<string> trainers;
            private bool alwaysActive;
            private string[] options = null;
            private System.Tuple<float, float> rangeFloat = null;
            private System.Tuple<int, int> rangeInt = null;
            private bool path = false;

            public string Help => help;
            public string Label => label;
            public List<string> Trainers => trainers;
            public bool AlwaysActive => alwaysActive;
            public bool Active => alwaysActive;
            public object Value => value;
            public string[] Options => options;
            public System.Tuple<float, float> RangeFloat => rangeFloat;
            public System.Tuple<int, int> RangeInt => rangeInt;
            public bool Path => path;

            public Entry(
                string value,
                string help = null,
                string label = null,
                List<string> trainers = null,
                bool alwaysActive = false,
                string[] options = null,
                int selectedIndex = 0,
                bool path = false,
                bool? active = null
            )
            {
                this.type = typeof(string);
                this.value = value;
                this.options = options;
                this.selectedIndex = selectedIndex;
                this.path = path;
                AssignCommonProps(
                    label,
                    help,
                    trainers,
                    alwaysActive,
                    active
                );
            }
            public Entry(
                bool value,
                string help = null,
                string label = null,
                List<string> trainers = null,
                bool alwaysActive = false,
                bool? active = null)
            {
                this.type = typeof(bool);
                this.value = value.ToString();
                AssignCommonProps(
                    label,
                    help,
                    trainers,
                    alwaysActive,
                    active
                );
            }

            public Entry(
                int value,
                string help = null,
                string label = null,
                List<string> trainers = null,
                bool alwaysActive = false,
                System.Tuple<int, int> range = null,
                bool? active = null
            )
            {
                this.type = typeof(int);
                this.rangeInt = range;
                this.value = value.ToString();
                AssignCommonProps(
                    label,
                    help,
                    trainers,
                    alwaysActive,
                    active
                );
            }
            public Entry(
                float value,
                string help = null,
                string label = null,
                List<string> trainers = null,
                bool alwaysActive = false,
                System.Tuple<float, float> range = null,
                bool? active = null)
            {
                this.type = typeof(float);
                this.rangeFloat = range;
                this.value = value.ToString();
                AssignCommonProps(
                    label,
                    help,
                    trainers,
                    alwaysActive,
                    active
                );
            }
            private void AssignCommonProps(
                string label = null,
                string help = null,
                List<string> trainers = null,
                bool alwaysActive = false,
                bool? active = null)
            {
                this.label = label;
                this.help = help;
                this.active = alwaysActive ? true : active == null ? true : System.Convert.ToBoolean(active);
                this.trainers = trainers != null ? trainers : defaultTrainers;
                this.alwaysActive = alwaysActive;
            }
        }
        public void WriteFile()
        {
            int indent = 0;
            string indentation()
            {
                return new string(' ', 2 * indent);
            }

            if (!FileExists) return;
            File.WriteAllText(Path, string.Empty);
            var writer = new StreamWriter(Path, true);
            WriteLines();
            writer.Close();

            void SettingsLine(Settings settings)
            {
                if (!settings.AlwaysActive && !settings.Active) return;
                writer.WriteLine(indentation() + settings.Key + ":");
            }

            void EntryLineAsKey(Entry entry)
            {
                writer.WriteLine(indentation() + entry.Value + ":");
            }

            void EntryLines(Dictionary<string, Entry> entries, string trainer_type = null)
            {
                foreach (var entry in entries)
                {
                    if (!new[] { "ppo_only", "sac_only" }.Contains(entry.Key))
                        EntryLine(entry.Value, entry.Key, trainer_type);
                }
            }
            void WriteNetworkSettings(TrainerSettings.NetworkSettings network_settings)
            {
                if (network_settings.inUse)
                {
                    SettingsLine(network_settings);
                    indent++;
                    EntryLines(network_settings.network_settings);

                    SettingsLine(network_settings.memory);
                    indent++;
                    EntryLines(network_settings.memory.memory);
                    indent--;
                    indent--;
                }
            }

            void WriteBehaviour(TrainerSettings behaviour)
            {
                var trainer_type = behaviour.trainer_settings["trainer_type"].value;
                indent++;
                EntryLines(behaviour.trainer_settings);
                if (defaultSettings.hyperparameters.inUse)
                {
                    SettingsLine(behaviour.hyperparameters);
                    indent++;
                    EntryLines(behaviour.hyperparameters.hyperparameters);
                    indent--;
                    WriteNetworkSettings(behaviour.network_settings);
                }
                if (defaultSettings.reward_signals.inUse)
                {
                    SettingsLine(behaviour.reward_signals);
                    indent++;
                    EntryLines(behaviour.reward_signals.reward_signals);
                    if (defaultSettings.reward_signals.extrinsic.inUse)
                    {
                        SettingsLine(behaviour.reward_signals.extrinsic);
                        indent++;
                        EntryLines(behaviour.reward_signals.extrinsic.extrinsic);
                        indent--;
                    }
                    if (defaultSettings.reward_signals.curiosity.inUse)
                    {
                        SettingsLine(behaviour.reward_signals.curiosity);
                        indent++;
                        EntryLines(behaviour.reward_signals.curiosity.curiosity);
                        SettingsLine(behaviour.reward_signals.curiosity.network_settings);
                        indent++;
                        EntryLines(behaviour.reward_signals.curiosity.network_settings.network_settings);
                        indent--;
                        indent--;
                    }
                    if (defaultSettings.reward_signals.gail.inUse)
                    {
                        SettingsLine(behaviour.reward_signals.gail);
                        indent++;
                        EntryLines(behaviour.reward_signals.gail.gail);
                        SettingsLine(behaviour.reward_signals.gail.network_settings);
                        indent++;
                        EntryLines(behaviour.reward_signals.gail.network_settings.network_settings);
                        indent--;
                        indent--;
                    }
                    if (defaultSettings.reward_signals.rnd.inUse)
                    {
                        SettingsLine(behaviour.reward_signals.rnd);
                        indent++;
                        EntryLines(behaviour.reward_signals.rnd.rnd);
                        SettingsLine(behaviour.reward_signals.rnd.network_settings);
                        indent++;
                        EntryLines(behaviour.reward_signals.rnd.network_settings.network_settings);
                        indent--;
                        indent--;
                    }
                    indent--;
                }
                if (defaultSettings.behavior_cloning.inUse)
                {
                    SettingsLine(behaviour.behavior_cloning);
                    indent++;
                    EntryLines(behaviour.behavior_cloning.behavior_cloning);
                    indent--;
                }
                if (defaultSettings.self_play.inUse)
                {
                    SettingsLine(behaviour.self_play);
                    indent++;
                    EntryLines(behaviour.self_play.self_play);
                    indent--;
                }
                indent--;
            }

            void WriteLines()
            {
                if (defaultSettings.inUse)
                {
                    SettingsLine(defaultSettings);
                    WriteBehaviour(defaultSettings);
                }
                if (multipleBehaviours.behaviors.Count() != 0)
                {
                    SettingsLine(multipleBehaviours);
                    indent++;
                    foreach (var behavior in multipleBehaviours.behaviors)
                    {
                        EntryLineAsKey(behavior.behavior_name["behavior_name"]);
                        WriteBehaviour(behavior.behaviour);
                    }
                    indent--;
                }
                if (checkpointSettings.inUse)
                {
                    SettingsLine(checkpointSettings);
                    indent++;
                    EntryLines(checkpointSettings.checkpoint_settings);
                    indent--;
                }
                if (engineSettings.inUse)
                {
                    SettingsLine(engineSettings);
                    indent++;
                    EntryLines(engineSettings.engine_settings);
                    indent--;
                }
                if (torchSettings.inUse)
                {
                    SettingsLine(torchSettings);
                    indent++;
                    EntryLines(torchSettings.torch_settings);
                    indent--;
                }

                EntryLine(debug["debug"], "debug");
            }

            void EntryLine(Entry entry, string key, string trainer_type = null)
            {
                var wrongTrainer = false;
                if (trainer_type != null)
                    wrongTrainer = !entry.Trainers.Contains((string) trainer_type);
                var excludeEntry = !entry.AlwaysActive && !entry.active | wrongTrainer;
                if (excludeEntry) return;

                switch (entry.type.Name)
                {
                    case nameof(System.String):
                        if ((string) entry.Value == "" || entry.Value == null) break;
                        writer.WriteLine(indentation() + key + ": " + entry.Value);
                        break;
                    case nameof(System.Int32):
                        writer.WriteLine(indentation() + key + ": " + entry.Value);
                        break;
                    case nameof(System.Single):
                        writer.WriteLine(indentation() + key + ": " + entry.Value);
                        break;
                    case nameof(System.Boolean):
                        writer.WriteLine(indentation() + key + ": " + entry.Value.ToString().ToLower());
                        break;
                }
            }
        }

        private void ReadDataInToClass(Dictionary<string, Entry> behavior, string key, string value)
        {
            if (behavior.ContainsKey(key))
            {
                behavior[key].value = value;
                behavior[key].active = true;
            }
            else
                behavior[key] = new Entry(value);
        }

        private void ReadNetworkSettings(TrainerSettings.NetworkSettings network_settings, TrainerSettings behavior, string value, string key, string type)
        {
            if (type == "network_settings")
            {
                ReadDataInToClass(behavior.network_settings.network_settings, key, value);
                behavior.network_settings.inUse = true;
            }
            else if (type == "memory")
            {
                ReadDataInToClass(behavior.network_settings.memory.memory, key, value);
                behavior.network_settings.memory.inUse = true;
            }
        }

        private void ReadBehavior(Dictionary<int, string> indentLevel, TrainerSettings behavior, string value, string key)
        {
            if (indentLevel.ContainsKey(1))
                switch (indentLevel[1])
                {
                    case "hyperparameters":
                        ReadDataInToClass(behavior.hyperparameters.hyperparameters, key, value);
                        behavior.hyperparameters.inUse = true;
                        break;
                    case "network_settings":
                        ReadNetworkSettings(behavior.network_settings, behavior, value, key, indentLevel.ContainsKey(2) ? "memory" : "network_settings");
                        break;
                    case "reward_signals":
                        if (indentLevel.ContainsKey(2))
                        {
                            switch (indentLevel[2])
                            {
                                case "extrinsic":
                                    ReadDataInToClass(behavior.reward_signals.extrinsic.extrinsic, key, value);
                                    behavior.reward_signals.extrinsic.inUse = true;
                                    break;
                                case "curiosity":
                                    if (indentLevel.ContainsKey(3))
                                        ReadNetworkSettings(behavior.reward_signals.curiosity.network_settings, behavior, value, key, indentLevel.ContainsKey(4) ? "memory" : "network_settings");
                                    else
                                    {
                                        ReadDataInToClass(behavior.reward_signals.curiosity.curiosity, key, value);
                                        behavior.reward_signals.curiosity.inUse = true;
                                    }
                                    break;
                                case "gail":
                                    if (indentLevel.ContainsKey(3))
                                        ReadNetworkSettings(behavior.reward_signals.gail.network_settings, behavior, value, key, indentLevel.ContainsKey(4) ? "memory" : "network_settings");
                                    else
                                    {
                                        ReadDataInToClass(behavior.reward_signals.gail.gail, key, value);
                                        behavior.reward_signals.gail.inUse = true;
                                    }
                                    break;
                                case "rnd":
                                    if (indentLevel.ContainsKey(3))
                                        ReadNetworkSettings(behavior.reward_signals.rnd.network_settings, behavior, value, key, indentLevel.ContainsKey(4) ? "memory" : "network_settings");
                                    else
                                    {
                                        ReadDataInToClass(behavior.reward_signals.rnd.rnd, key, value);
                                        behavior.reward_signals.rnd.inUse = true;
                                    }
                                    break;
                            }
                        }
                        else
                            ReadDataInToClass(behavior.reward_signals.reward_signals, key, value);
                        behavior.reward_signals.inUse = true;
                        break;
                    case "behavior_cloning":
                        ReadDataInToClass(behavior.behavior_cloning.behavior_cloning, key, value);
                        behavior.behavior_cloning.inUse = true;
                        break;
                    case "self_play":
                        ReadDataInToClass(behavior.self_play.self_play, key, value);
                        behavior.self_play.inUse = true;
                        break;
                }
            else
            {
                ReadDataInToClass(behavior.trainer_settings, key, value);
                behavior.inUse = true;
            }
        }

        public void ReadFile()
        {
            var configKeys = new List<string>();
            defaultSettings = new TrainerSettings("default_settings", alwaysActive: true, inUse: false, newActive: false);
            multipleBehaviours = new MultipleBehaviours("behaviors", alwaysActive: false);
            engineSettings = new EngineSettings(inUse: false, newActive: false);
            checkpointSettings = new CheckpointSettings(inUse: false, newActive: false);
            torchSettings = new TorchSettings(inUse: false, newActive: false);
            using (StreamReader sr = new StreamReader(Path))
            {
                string line = "";
                var indentLevel = new Dictionary<int, string>() { };
                int indent = 0;
                int behavior = -1;
                while ((line = sr.ReadLine()) != null)
                {
                    var spiltFile = line.Split(':');
                    int indentInFile = spiltFile[0].TakeWhile(System.Char.IsWhiteSpace).Count() / 2;
                    string key = Regex.Replace(spiltFile[0], @"\s+", "");
                    string value = Regex.Replace(line.Replace(spiltFile[0] + ":", ""), @"\s+", "");
                    if (indentInFile < indent)
                    {
                        for (int i = indentInFile; i <= indent; i++)
                        {
                            indentLevel.Remove(i);
                        }
                    }
                    if (value == "")
                    {
                        if (indentLevel.Count() == 1 && indentLevel[0] == "behaviors")
                        {
                            multipleBehaviours.behaviors.Add(new TrainerSettingsMultipleBehaviour(key, newActive: false));
                            behavior++;
                        }
                        indentLevel[indentInFile] = key;
                        indent = indentInFile;
                        continue;
                    }
                    indent = indentInFile;
                    switch (indentLevel.ContainsKey(0) ? indentLevel[0] : "")
                    {
                        case "default_settings":
                            ReadBehavior(indentLevel, defaultSettings, value, key);
                            break;
                        case "behaviors":
                            var behaviorsIndentLevel = new Dictionary<int, string>(indentLevel);
                            behaviorsIndentLevel.Remove(0);
                            ReadBehavior(behaviorsIndentLevel, multipleBehaviours.behaviors[behavior].behaviour, value, key);
                            break;
                        case "engine_settings":
                            ReadDataInToClass(engineSettings.engine_settings, key, value);
                            engineSettings.inUse = true;
                            break;
                        case "checkpoint_settings":
                            ReadDataInToClass(checkpointSettings.checkpoint_settings, key, value);
                            checkpointSettings.inUse = true;
                            break;
                        case "torch_settings":
                            ReadDataInToClass(torchSettings.torch_settings, key, value);
                            torchSettings.inUse = true;
                            break;
                        default:
                            debug[key].value = value;
                            break;
                    }

                }
            }
        }
    }
}

#endif