#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;

public class Config : MonoBehaviour
{
    public static List<string> defaultTrainers = new List<string> { "ppo", "sac", "poca" };

    [HideInInspector]
    public string filename = "config";
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
    public Entry debug = new Entry("debug", false);

    #region Settings
    [System.Serializable]
    public class TrainerSettings : Settings
    {
        public TrainerSettings(string key, bool alwaysActive, bool inUse = true) : base(key) { }

        [SerializeField]
        public List<Entry> trainer_settings = new List<Entry>() {
         new Entry(
            "trainer_type", "ppo",
            alwaysActive: true,
            help: "(default = ppo) The type of trainer to use: ppo, sac, or poca.",
            options: new string[] { "ppo", "sac", "poca" }
        ),
        new Entry(
            "summary_freq", 50000,
           active: true,
           help:
            "(default = 50000) Number of experiences that needs to be collected before generating and displaying training statistics. This determines the granularity of the graphs in Tensorboard."
        ),
        new Entry(
            "time_horizon", 64,
           active: true,
           range: new System.Tuple<int, int>(32, 2048),
            help:
            "(default = 64) How many steps of experience to collect per-agent before adding it to the experience buffer. When this limit is reached before the end of an episode, a value estimate is used to predict the overall expected reward from the agent's current state. As such, this parameter trades off between a less biased, but higher variance estimate (long time horizon) and more biased, but less varied estimate (short time horizon). In cases where there are frequent rewards within an episode, or episodes are prohibitively large, a smaller number can be more ideal. This number should be large enough to capture all the important behavior within a sequence of an agent's actions.\n\nTypical range: 32 - 2048"
        ),
         new Entry(
            "max_steps", 500000,
            active: true,
            range: new System.Tuple<int, int>(500000, 10000000),
            help:
            "(default = 500000) Total number of steps (i.e., observation collected and action taken) that must be taken in the environment (or across all environments if using multiple in parallel) before ending the training process. If you have multiple agents with the same behavior name within your environment, all steps taken by those agents will contribute to the same max_steps count.\n\nTypical range: 5e5 - 1e7"
        ),
         new Entry(
            "keep_checkpoints", 5,
            active: true,
            help:
            "(default = 5) The maximum number of model checkpoints to keep. Checkpoints are saved after the number of steps specified by the checkpoint_interval option. Once the maximum number of checkpoints has been reached, the oldest checkpoint is deleted when saving a new checkpoint."
        ),
        new Entry(
            "checkpoint_interval", 500000,
           active: true,
           help:
            "(default = 500000) The number of experiences collected between each checkpoint by the trainer. A maximum of keep_checkpoints checkpoints are saved before old ones are deleted. Each checkpoint saves the .onnx files in results/ folder."
        ),
        new Entry(
            "init_path", "",
           active: true,
           help:
            "(default = None) Initialize trainer from a previously saved model. Note that the prior run should have used the same trainer configurations as the current run, and have been saved with the same version of ML-Agents. You should provide the full path to the folder where the checkpoints were saved, e.g. ./models/{run-id}/{behavior_name}. This option is provided in case you want to initialize different behaviors from different runs; in most cases, it is sufficient to use the --initialize-from CLI parameter to initialize all models from the same run."
        ),
        new Entry(
            "threaded", true,
           active: true,
           help:
            "(default = true) By default, model updates can happen while the environment is being stepped. This violates the on-policy assumption of PPO slightly in exchange for a training speedup. To maintain the strict on-policyness of PPO, you can disable parallel updates by setting threaded to false. There is usually no reason to turn threaded off for SAC."
        )};

        public NetworkSettings network_settings = new NetworkSettings();
        public Hyperparameters hyperparameters = new Hyperparameters();

        [System.Serializable]
        public class Hyperparameters : Settings
        {
            public Hyperparameters(string key = "hyperparameters") : base(key) { }
            public List<Entry> hyperparameters_entries_base = new List<Entry>()
            {
                 new Entry(
                    "learning_rate", 0.0003f,
                    active: true,
                    range: new System.Tuple<float, float>(0.001f, 0.00001f),
                    help:
                    "(default = 3e-4) Initial learning rate for gradient descent. Corresponds to the strength of each gradient descent update step. This should typically be decreased if training is unstable, and the reward does not consistently increase.\n\nTypical range: 1e-5 - 1e-3"
                ),
                 new Entry(
                    "batch_size", 256,
                    active: true,
                    help:
                    "Number of experiences in each iteration of gradient descent. This should always be multiple times smaller than buffer_size. If you are using continuous actions, this value should be large (on the order of 1000s). If you are using only discrete actions, this value should be smaller (on the order of 10s).\n\nTypical range: (Continuous - PPO): 512 - 5120; (Continuous - SAC): 128 - 1024; (Discrete, PPO & SAC): 32 - 512."
                ),
                 new Entry(
                    "buffer_size", 10240,
                    active: true,
                    help:
                    "(default = 10240 for PPO and 50000 for SAC)\n\nPPO: Number of experiences to collect before updating the policy model. Corresponds to how many experiences should be collected before we do any learning or updating of the model. This should be multiple times larger than batch_size. Typically a larger buffer_size corresponds to more stable training updates.\n\nSAC: The max size of the experience buffer - on the order of thousands of times longer than your episodes, so that SAC can learn from old as well as new experiences."
                ),
                 new Entry(
                    "learning_rate_schedule", "linear",
                    active: true,
                    help:
                    "(default = linear for PPO and constant for SAC) Determines how learning rate changes over time. For PPO, we recommend decaying learning rate until max_steps so learning converges more stably. However, for some cases (e.g. training for an unknown amount of time) this feature can be disabled. For SAC, we recommend holding learning rate constant so that the agent can continue to learn until its Q function converges naturally.\n\nlinear decays the learning_rate linearly, reaching 0 at max_steps, while constant keeps the learning rate constant for the entire training run."
                )
            };
            public List<Entry> hyperparameters_entries_ppo = new List<Entry>(){
             new Entry(
                "beta", 0.005f,
                active: true,
                range: new System.Tuple<float, float>(0.01f, 0.0001f),
                trainers: new List<string> { "ppo", "poca" },
                help:
                "(default = 5.0e-3) Strength of the entropy regularization, which makes the policy \"more random.\" This ensures that agents properly explore the action space during training. Increasing this will ensure more random actions are taken. This should be adjusted such that the entropy (measurable from TensorBoard) slowly decreases alongside increases in reward. If entropy drops too quickly, increase beta. If entropy drops too slowly, decrease beta.\n\nTypical range: 1e-4 - 1e-2"
            ),
             new Entry(
                "epsilon", 0.2f,
                active: true,
                trainers: new List<string> { "ppo", "poca" },
                range: new System.Tuple<float, float>(0.1f, 0.3f),
                help:
                "(default = 0.2) Influences how rapidly the policy can evolve during training. Corresponds to the acceptable threshold of divergence between the old and new policies during gradient descent updating. Setting this value small will result in more stable updates, but will also slow the training process.\n\nTypical range: 0.1 - 0.3"
            ),
             new Entry( // Not sure if `lambd` is a typo?
                "lambd", 0.95f,
                active: true,
                range: new System.Tuple<float, float>(0.9f, 0.95f),
                trainers: new List<string> { "ppo", "poca" },
                help:
                "(default = 0.95) Regularization parameter (lambda) used when calculating the Generalized Advantage Estimate (GAE). This can be thought of as how much the agent relies on its current value estimate when calculating an updated value estimate. Low values correspond to relying more on the current value estimate (which can be high bias), and high values correspond to relying more on the actual rewards received in the environment (which can be high variance). The parameter provides a trade-off between the two, and the right value can lead to a more stable training process.\n\nTypical range: 0.9 - 0.95"
            ),
             new Entry(
                "num_epoch", 3,
                active: true,
                trainers: new List<string> { "ppo", "poca" },
                range: new System.Tuple<int, int>(3, 10),
                help:
                "(default = 3) Number of passes to make through the experience buffer when performing gradient descent optimization.The larger the batch_size, the larger it is acceptable to make this. Decreasing this will ensure more stable updates, at the cost of slower learning.\n\nTypical range: 1000 - 10000"
            ),};
            public List<Entry> hyperparameters_entries_sac = new List<Entry>(){
             new Entry(
                "buffer_init_steps", 0,
                active: true,
                trainers: new List<string> { "sac" },
                range: new System.Tuple<int, int>(1000, 10000),
                help:
                "(default = 0) Number of experiences to collect into the buffer before updating the policy model. As the untrained policy is fairly random, pre-filling the buffer with random actions is useful for exploration. Typically, at least several episodes of experiences should be pre-filled.\n\nTypical range: 1000 - 10000"
            ),
             new Entry(
                "init_entcoef", 1.0f,
                active: true,
                trainers: new List<string> { "sac" },
                range: new System.Tuple<float, float>(0.05f, 1f),
                help:
                "(default = 1.0) How much the agent should explore in the beginning of training. Corresponds to the initial entropy coefficient set at the beginning of training. In SAC, the agent is incentivized to make its actions entropic to facilitate better exploration. The entropy coefficient weighs the true reward with a bonus entropy reward. The entropy coefficient is automatically adjusted to a preset target entropy, so the init_entcoef only corresponds to the starting value of the entropy bonus. Increase init_entcoef to explore more in the beginning, decrease to converge to a solution faster.\n\nTypical range: (Continuous): 0.5 - 1.0; (Discrete): 0.05 - 0.5"
            ),
             new Entry(
                "save_replay_buffer", false,
                active: true,
                trainers: new List<string> { "sac" },
                help:
                "(default = false) Whether to save and load the experience replay buffer as well as the model when quitting and re-starting training. This may help resumes go more smoothly, as the experiences collected won't be wiped. Note that replay buffers can be very large, and will take up a considerable amount of disk space. For that reason, we disable this feature by default."
            ),
             new Entry(
                "tau", 0.005f,
                active: true,
                trainers: new List<string> { "sac" },
                range: new System.Tuple<float, float>(0.05f, 0.01f),
                help:
                "(default = 0.005) How aggressively to update the target network used for bootstrapping value estimation in SAC. Corresponds to the magnitude of the target Q update during the SAC model update. In SAC, there are two neural networks: the target and the policy. The target network is used to bootstrap the policy's estimate of the future rewards at a given state, and is fixed while the policy is being updated. This target is then slowly updated according to tau. Typically, this value should be left at 0.005. For simple problems, increasing tau to 0.01 might reduce the time it takes to learn, at the cost of stability.\n\nTypical range: 0.005 - 0.01"
            ),
             new Entry(
                "steps_per_update", 1,
                active: true,
                trainers: new List<string> { "sac" },
                range: new System.Tuple<int, int>(1, 20),
                help:
                "(default = 1) Average ratio of agent steps (actions) taken to updates made of the agent's policy. In SAC, a single \"update\" corresponds to grabbing a batch of size batch_size from the experience replay buffer, and using this mini batch to update the models. Note that it is not guaranteed that after exactly steps_per_update steps an update will be made, only that the ratio will hold true over many steps. Typically, steps_per_update should be greater than or equal to 1. Note that setting steps_per_update lower will improve sample efficiency (reduce the number of steps required to train) but increase the CPU time spent performing updates. For most environments where steps are fairly fast (e.g. our example environments) steps_per_update equal to the number of agents in the scene is a good balance. For slow environments (steps take 0.1 seconds or more) reducing steps_per_update may improve training speed. We can also change steps_per_update to lower than 1 to update more often than once per step, though this will usually result in a slowdown unless the environment is very slow.\n\nTypical range: 1 - 20"
            ),
             new Entry(
                "reward_signal_num_update", "steps_per_update",
                active: true,
                trainers: new List<string> { "sac" },
                help:
                "(default = steps_per_update) Number of steps per mini batch sampled and used for updating the reward signals. By default, we update the reward signals once every time the main policy is updated. However, to imitate the training procedure in certain imitation learning papers (e.g. Kostrikov et. al, Blondé et. al), we may want to update the reward signal (GAIL) M times for every update of the policy. We can change steps_per_update of SAC to N, as well as reward_signal_steps_per_update under reward_signals to N / M to accomplish this. By default, reward_signal_steps_per_update is set to steps_per_update."
            )
        };
        }
        [System.Serializable]
        public class NetworkSettings : Settings
        {
            public NetworkSettings(string key = "network_settings") : base(key) { }
            public List<Entry> network_settings_entries = new List<Entry>()
            {
             new Entry(
                "hidden_units", 128,
                active: true,
                range: new System.Tuple<int, int>(32, 512),
                help:
                "(default = 128) Number of units in the hidden layers of the neural network. Correspond to how many units are in each fully connected layer of the neural network. For simple problems where the correct action is a straightforward combination of the observation inputs, this should be small. For problems where the action is a very complex interaction between the observation variables, this should be larger.\n\nTypical range: 32 - 512"
            ),
             new Entry(
                "num_layers", 2,
                active: true,
                range: new System.Tuple<int, int>(1, 3),
                help:
                "(default = 2) The number of hidden layers in the neural network. Corresponds to how many hidden layers are present after the observation input, or after the CNN encoding of the visual observation. For simple problems, fewer layers are likely to train faster and more efficiently. More layers may be necessary for more complex control problems.\n\nTypical range: 1 - 3"
            ),
             new Entry(
                "normalize", false,
                active: true,
                help:
                "(default = false) Whether normalization is applied to the vector observation inputs. This normalization is based on the running average and variance of the vector observation. Normalization can be helpful in cases with complex continuous control problems, but may be harmful with simpler discrete control problems."
            ),
             new Entry(
                "vis_encode_type", "simple",
                active: true,
                help:
                "(default = simple) Encoder type for encoding visual observations.\n\nsimple (default) uses a simple encoder which consists of two convolutional layers, nature_cnn uses the CNN implementation proposed by Mnih et al., consisting of three convolutional layers, and resnet uses the IMPALA Resnet consisting of three stacked layers, each with two residual blocks, making a much larger network than the other two. match3 is a smaller CNN (Gudmundsoon et al.) that is optimized for board games, and can be used down to visual observation sizes of 5x5."
            ),
            };
        }
    }
    public class TrainerSettingsMultipleBehaviour
    {
        public TrainerSettings Behaviour = new TrainerSettings("default_settings", alwaysActive: true);
        public Entry behavior_name;
        public TrainerSettingsMultipleBehaviour(string behaviorName)
        {
            this.behavior_name = new Entry(
                        "behavior_name", behaviorName,
                        alwaysActive: true,
                        help: "Name the behavior have to be unique"
                    );
        }
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
        public EngineSettings(string key = "engine_settings", bool inUse = true) : base(key) { }
        public List<Entry> engine_settings = new List<Entry>() {
         new Entry("time_scale", 1.0f),
         new Entry("quality_level", 5),
         new Entry("target_frame_rate", -1),
         new Entry("capture_frame_rate", 60),
         new Entry("no_graphics", false),
         new Entry("width", 84),
         new Entry("height", 84)
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
        public CheckpointSettings(string key = "checkpoint_settings", bool inUse = true) : base(key) { }
        public List<Entry> checkpoint_settings = new List<Entry>() {
         new Entry("run_id", "mlady"),
         new Entry("initialize_from", ""),
         new Entry("load_model", false),
         new Entry("resume", false),
         new Entry("force", false),
         new Entry("train_model", false),
         new Entry("inference", false),
         new Entry("results_dir", "Results")
            };
    }
    [System.Serializable]
    public class TorchSettings : Settings
    {
        public TorchSettings(string key = "torch_settings", bool inUse = true) : base(key) { }
        public List<Entry> device = new List<Entry>() { new Entry("device", "cuda") };
    }
    #endregion

    public interface IYAMLLine
    {
        string Key { get; }
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
        public bool inUse = true;

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

        private string key;
        private string help;
        private string label;
        private List<string> trainers;
        private bool alwaysActive;
        private string[] options = null;
        private System.Tuple<float, float> rangeFloat = null;
        private System.Tuple<int, int> rangeInt = null;

        public string Key => key;
        public string Help => help;
        public string Label => label;
        public List<string> Trainers => trainers;
        public bool AlwaysActive => alwaysActive;
        public bool Active => alwaysActive;
        public object Value => value;
        public string[] Options => options;
        public System.Tuple<float, float> RangeFloat => rangeFloat;
        public System.Tuple<int, int> RangeInt => rangeInt;

        public Entry(
            string key,
            string value,
            string help = null,
            string label = null,
            bool active = true,
            List<string> trainers = null,
            bool alwaysActive = false,
            string[] options = null,
            int selectedIndex = 0
        )
        {
            this.type = typeof(string);
            this.value = value;
            this.options = options;
            this.selectedIndex = selectedIndex;
            AssignCommonProps(
                key,
                label,
                help,
                active,
                trainers,
                alwaysActive
            );
        }
        public Entry(
            string key,
            bool value,
            string help = null,
            string label = null,
            bool active = true,
            List<string> trainers = null,
            bool alwaysActive = false
        )
        {
            this.type = typeof(bool);
            this.value = value.ToString();
            AssignCommonProps(
                key,
                label,
                help,
                active,
                trainers,
                alwaysActive
            );
        }

        public Entry(
            string key,
            int value,
            string help = null,
            string label = null,
            bool active = true,
            List<string> trainers = null,
            bool alwaysActive = false,
            System.Tuple<int, int> range = null
        )
        {
            this.type = typeof(int);
            this.rangeInt = range;
            this.value = value.ToString();
            AssignCommonProps(
                key,
                label,
                help,
                active,
                trainers,
                alwaysActive
            );
        }
        public Entry(
            string key,
            float value,
            string help = null,
            string label = null,
            bool active = true,
            List<string> trainers = null,
            bool alwaysActive = false,
            System.Tuple<float, float> range = null
        )
        {
            this.type = typeof(float);
            this.rangeFloat = range;
            this.value = value.ToString();
            AssignCommonProps(
                key,
                label,
                help,
                active,
                trainers,
                alwaysActive
            );
        }
        private void AssignCommonProps(
            string key,
            string label = null,
            string help = null,
            bool active = true,
            List<string> trainers = null,
            bool alwaysActive = false
        )
        {
            this.key = key;
            this.label = label != null ? label : key;
            this.help = help;
            this.active = alwaysActive ? true : active;
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

        void EntryLines(List<Entry> entries, string trainer_type = null)
        {
            foreach (var entry in entries)
            {
                EntryLine(entry, trainer_type);
            }
        }

        void WriteBehaviour(TrainerSettings behaviour)
        {
            var trainer_type = behaviour.trainer_settings[0].value;
            indent++;
            EntryLines(behaviour.trainer_settings);

            SettingsLine(behaviour.hyperparameters);
            indent++;
            EntryLines(behaviour.hyperparameters.hyperparameters_entries_base);
            EntryLines(behaviour.hyperparameters.hyperparameters_entries_ppo, trainer_type);
            EntryLines(behaviour.hyperparameters.hyperparameters_entries_sac, trainer_type);
            indent--;
            SettingsLine(behaviour.network_settings);
            indent++;
            EntryLines(behaviour.network_settings.network_settings_entries);
            indent--;
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
                    EntryLineAsKey(behavior.behavior_name);
                    WriteBehaviour(behavior.Behaviour);
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
                EntryLines(torchSettings.device);
                indent--;
            }

            EntryLine(debug);
        }

        void EntryLine(Entry entry, string trainer_type = null)
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
                    writer.WriteLine(indentation() + entry.Key + ": " + entry.Value);
                    break;
                case nameof(System.Int32):
                    writer.WriteLine(indentation() + entry.Key + ": " + entry.Value);
                    break;
                case nameof(System.Single):
                    writer.WriteLine(indentation() + entry.Key + ": " + entry.Value);
                    break;
                case nameof(System.Boolean):
                    writer.WriteLine(indentation() + entry.Key + ": " + entry.Value.ToString().ToLower());
                    break;
            }
        }
    }
    private void UpdateBehaviorsClass(Settings settings, Entry entry)
    {
        if (readData[settings.Key].ContainsKey(entry.Key))
            entry.value = readData[settings.Key][entry.Key].ToString();
        else entry.active = false;
    }
    private void UpdateMultipleBehavioursClass(Dictionary<string, Dictionary<string, Dictionary<string, string>>> data, TrainerSettingsMultipleBehaviour settings, Entry entry)
    {
        if (data["behaviors"][settings.behavior_name.value].ContainsKey(entry.Key))
            entry.value = data["behaviors"][settings.behavior_name.value][entry.Key].ToString();
        else entry.active = false;
    }

    private void WriteToClass(Settings settings, List<Entry> entries)
    {
        if (settings.inUse)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (readData[settings.Key].ContainsKey(entry.Key))
                    entry.value = readData[settings.Key][entry.Key].ToString();
                else entry.active = false;
            }
        }
    }
    private void WriteBehaviorsToClass(TrainerSettings settings)
    {

        for (int i = 0; i < settings.trainer_settings.Count; i++)
        {
            UpdateBehaviorsClass(settings, settings.trainer_settings[i]);
        }
        for (int i = 0; i < settings.hyperparameters.hyperparameters_entries_base.Count; i++)
        {
            UpdateBehaviorsClass(settings, settings.hyperparameters.hyperparameters_entries_base[i]);
        }
        for (int i = 0; i < settings.hyperparameters.hyperparameters_entries_ppo.Count; i++)
        {
            UpdateBehaviorsClass(settings, settings.hyperparameters.hyperparameters_entries_ppo[i]);
        }
        for (int i = 0; i < settings.hyperparameters.hyperparameters_entries_sac.Count; i++)
        {
            UpdateBehaviorsClass(settings, settings.hyperparameters.hyperparameters_entries_sac[i]);
        }
        for (int i = 0; i < settings.network_settings.network_settings_entries.Count; i++)
        {
            UpdateBehaviorsClass(settings, settings.network_settings.network_settings_entries[i]);
        }

    }

    public void ReadFile()
    {
        defaultSettings = new TrainerSettings("default_settings", alwaysActive: true, inUse: false);
        multipleBehaviours = new MultipleBehaviours("behaviors", alwaysActive: false);
        engineSettings = new EngineSettings(inUse: false);
        checkpointSettings = new CheckpointSettings(inUse: false);
        torchSettings = new TorchSettings(inUse: false);
        using (StreamReader sr = new StreamReader(Path))
        {
            string line = "";
            string settingsType = "";
            string multipleBehavioursType = "";
            while ((line = sr.ReadLine()) != null)
            {
                var spiltFile = line.Split(':');
                string key = Regex.Replace(spiltFile[0], @"\s+", "");
                string value = Regex.Replace(spiltFile[1], @"\s+", "");
                switch (key)
                {
                    case "default_settings":
                        defaultSettings.inUse = true;
                        settingsType = "default_settings";
                        break;
                    case "behaviors":
                        settingsType = "behaviors";
                        break;
                    case "engine_settings":
                        engineSettings.inUse = true;
                        settingsType = "engine_settings";
                        break;
                    case "checkpoint_settings":
                        checkpointSettings.inUse = true;
                        settingsType = "checkpoint_settings";
                        break;
                    case "torch_settings":
                        torchSettings.inUse = true;
                        settingsType = "torch_settings";
                        break;
                    default:
                        break;
                }
                switch (settingsType)
                {
                    case "behaviors":
                        if (value == "" && key == "behaviors")
                            readDataMultipleBehaviours[settingsType] = new Dictionary<string, Dictionary<string, string>>();
                        else if (value == "" && !(new List<string> { "behaviors", "hyperparameters", "network_settings" }.Contains(key)))
                        {
                            multipleBehavioursType = key;
                            readDataMultipleBehaviours[settingsType][multipleBehavioursType] = new Dictionary<string, string>();
                        }
                        else if (key != "behaviors")
                            readDataMultipleBehaviours[settingsType][multipleBehavioursType][key] = value;
                        break;
                    default:
                        if (!readData.ContainsKey(settingsType))
                            readData[settingsType] = new Dictionary<string, string>();
                        else readData[settingsType][key] = value;
                        break;
                }
            }
        }
        if (defaultSettings.inUse)
        {
            WriteBehaviorsToClass(defaultSettings);
        }
        if (readDataMultipleBehaviours.ContainsKey("behaviors"))
        {
            int index = 0;
            foreach (KeyValuePair<string, Dictionary<string, string>> behaviors in readDataMultipleBehaviours["behaviors"])
            {
                multipleBehaviours.behaviors.Add(new Config.TrainerSettingsMultipleBehaviour(behaviors.Key));
                WriteBehaviorsToClass(multipleBehaviours.behaviors[index].Behaviour);
                var settings = multipleBehaviours.behaviors[index];
                for (int i = 0; i < settings.Behaviour.trainer_settings.Count; i++)
                {
                    UpdateMultipleBehavioursClass(readDataMultipleBehaviours, settings, settings.Behaviour.trainer_settings[i]);
                }
                for (int i = 0; i < settings.Behaviour.hyperparameters.hyperparameters_entries_base.Count; i++)
                {
                    UpdateMultipleBehavioursClass(readDataMultipleBehaviours, settings, settings.Behaviour.trainer_settings[i]);
                }
                for (int i = 0; i < settings.Behaviour.hyperparameters.hyperparameters_entries_ppo.Count; i++)
                {
                    UpdateMultipleBehavioursClass(readDataMultipleBehaviours, settings, settings.Behaviour.trainer_settings[i]);
                }
                for (int i = 0; i < settings.Behaviour.hyperparameters.hyperparameters_entries_sac.Count; i++)
                {
                    UpdateMultipleBehavioursClass(readDataMultipleBehaviours, settings, settings.Behaviour.trainer_settings[i]);
                }
                for (int i = 0; i < settings.Behaviour.network_settings.network_settings_entries.Count; i++)
                {
                    UpdateMultipleBehavioursClass(readDataMultipleBehaviours, settings, settings.Behaviour.trainer_settings[i]);
                }
                index++;
            }
        }
        WriteToClass(engineSettings, engineSettings.engine_settings);
        WriteToClass(checkpointSettings, checkpointSettings.checkpoint_settings);
        WriteToClass(torchSettings, torchSettings.device);

        // logging
        // foreach (KeyValuePair<string, Dictionary<string, string>> attachStat1 in readData)
        // {
        //     Debug.Log(attachStat1.Key);
        //     foreach (KeyValuePair<string, string> attachStat2 in attachStat1.Value)
        //     {
        //         Debug.Log(attachStat2.Key);
        //         Debug.Log(attachStat2.Value);
        //     }
        // }
        // foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, string>>> attachStat1 in readDataMultipleBehaviours)
        // {
        //     Debug.Log(attachStat1.Key);
        //     foreach (KeyValuePair<string, Dictionary<string, string>> attachStat2 in attachStat1.Value)
        //     {
        //         Debug.Log(attachStat2.Key);
        //         foreach (KeyValuePair<string, string> attachStat3 in attachStat2.Value)
        //         {
        //             Debug.Log(attachStat3.Key);
        //             Debug.Log(attachStat3.Value);

        //         }
        //     }
        // }
    }

}

#endif