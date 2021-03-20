#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Config : MonoBehaviour
{
    public bool showHelp = true;
    public bool showOutput = true;
    public bool disableYAMLOutput = false;
    public class Entries : Dictionary<string, Entry> { }
    public string trainer
    {
        get
        {
            return (string) entries["default_settings"].entries["trainer_type"].value;
        }
    }

    public Entries entries = new Entries
    {
        {
            "default_settings", new Entry("default_settings", new Entries
            {
                // Training Config Docs (behavior settings only)
                // https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-Configuration-File.md
                {
                    "trainer_type", new Entry(
                        "trainer_type", "ppo",
                        active: true,
                        help: "(default = ppo) The type of trainer to use: ppo, sac, or poca."
                    )
                },
                {
                    "summary_freq", new Entry(
                        "summary_freq", 50000,
                        active: true,
                        help:
                        "(default = 50000) Number of experiences that needs to be collected before generating and displaying training statistics. This determines the granularity of the graphs in Tensorboard."
                    )
                },
                {
                    "time_horizon", new Entry(
                        "time_horizon", 64,
                        active: true,
                        help:
                        "(default = 64) How many steps of experience to collect per-agent before adding it to the experience buffer. When this limit is reached before the end of an episode, a value estimate is used to predict the overall expected reward from the agent's current state. As such, this parameter trades off between a less biased, but higher variance estimate (long time horizon) and more biased, but less varied estimate (short time horizon). In cases where there are frequent rewards within an episode, or episodes are prohibitively large, a smaller number can be more ideal. This number should be large enough to capture all the important behavior within a sequence of an agent's actions.\n\nTypical range: 32 - 2048"
                    )
                },
                {
                    "max_steps", new Entry(
                        "max_steps", 500000,
                        active: true,
                        help:
                        "(default = 500000) Total number of steps (i.e., observation collected and action taken) that must be taken in the environment (or across all environments if using multiple in parallel) before ending the training process. If you have multiple agents with the same behavior name within your environment, all steps taken by those agents will contribute to the same max_steps count.\n\nTypical range: 5e5 - 1e7"
                    )
                },
                {
                    "keep_checkpoints", new Entry(
                        "keep_checkpoints", 5,
                        active: true,
                        help:
                        "(default = 5) The maximum number of model checkpoints to keep. Checkpoints are saved after the number of steps specified by the checkpoint_interval option. Once the maximum number of checkpoints has been reached, the oldest checkpoint is deleted when saving a new checkpoint."
                    )
                },
                {
                    "checkpoint_interval", new Entry(
                        "checkpoint_interval", 500000,
                        active: true,
                        help:
                        "(default = 500000) The number of experiences collected between each checkpoint by the trainer. A maximum of keep_checkpoints checkpoints are saved before old ones are deleted. Each checkpoint saves the .onnx files in results/ folder."
                    )
                },
                {
                    "init_path", new Entry(
                        "init_path", "",
                        active: true,
                        help:
                        "(default = None) Initialize trainer from a previously saved model. Note that the prior run should have used the same trainer configurations as the current run, and have been saved with the same version of ML-Agents. You should provide the full path to the folder where the checkpoints were saved, e.g. ./models/{run-id}/{behavior_name}. This option is provided in case you want to initialize different behaviors from different runs; in most cases, it is sufficient to use the --initialize-from CLI parameter to initialize all models from the same run."
                    )
                },
                {
                    "threaded", new Entry(
                        "threaded",
                        true,
                        active: true,
                        help:
                        "(default = true) By default, model updates can happen while the environment is being stepped. This violates the on-policy assumption of PPO slightly in exchange for a training speedup. To maintain the strict on-policyness of PPO, you can disable parallel updates by setting threaded to false. There is usually no reason to turn threaded off for SAC."
                    )
                },
                {
                    "hyperparameters", new Entry(
                        "hyperparameters", new Entries
                        {
                            {
                                "learning_rate", new Entry(
                                    "learning_rate", 0.0003f,
                                    active: true,
                                    help:
                                    "(default = 3e-4) Initial learning rate for gradient descent. Corresponds to the strength of each gradient descent update step. This should typically be decreased if training is unstable, and the reward does not consistently increase.\n\nTypical range: 1e-5 - 1e-3"
                                )
                            },
                            {
                                "batch_size", new Entry(
                                    "batch_size", 256,
                                    active: true,
                                    help:
                                    "Number of experiences in each iteration of gradient descent. This should always be multiple times smaller than buffer_size. If you are using continuous actions, this value should be large (on the order of 1000s). If you are using only discrete actions, this value should be smaller (on the order of 10s).\n\nTypical range: (Continuous - PPO): 512 - 5120; (Continuous - SAC): 128 - 1024; (Discrete, PPO & SAC): 32 - 512."
                                )
                            },
                            {
                                "buffer_size", new Entry(
                                    "buffer_size", 10240,
                                    active: true,
                                    help:
                                    "(default = 10240 for PPO and 50000 for SAC)\n\nPPO: Number of experiences to collect before updating the policy model. Corresponds to how many experiences should be collected before we do any learning or updating of the model. This should be multiple times larger than batch_size. Typically a larger buffer_size corresponds to more stable training updates.\n\nSAC: The max size of the experience buffer - on the order of thousands of times longer than your episodes, so that SAC can learn from old as well as new experiences."
                                )
                            },
                            {
                                "learning_rate_schedule", new Entry(
                                    "learning_rate_schedule", "linear",
                                    active: true,
                                    help:
                                    "(default = linear for PPO and constant for SAC) Determines how learning rate changes over time. For PPO, we recommend decaying learning rate until max_steps so learning converges more stably. However, for some cases (e.g. training for an unknown amount of time) this feature can be disabled. For SAC, we recommend holding learning rate constant so that the agent can continue to learn until its Q function converges naturally.\n\nlinear decays the learning_rate linearly, reaching 0 at max_steps, while constant keeps the learning rate constant for the entire training run."
                                )
                            },
                            {
                                "hidden_units", new Entry(
                                    "hidden_units", 128,
                                    active: true,
                                    help:
                                    "(default = 128) Number of units in the hidden layers of the neural network. Correspond to how many units are in each fully connected layer of the neural network. For simple problems where the correct action is a straightforward combination of the observation inputs, this should be small. For problems where the action is a very complex interaction between the observation variables, this should be larger.\n\nTypical range: 32 - 512"
                                )
                            },
                            {
                                "num_layers", new Entry(
                                    "num_layers", 2,
                                    active: true,
                                    help:
                                    "(default = 2) The number of hidden layers in the neural network. Corresponds to how many hidden layers are present after the observation input, or after the CNN encoding of the visual observation. For simple problems, fewer layers are likely to train faster and more efficiently. More layers may be necessary for more complex control problems.\n\nTypical range: 1 - 3"
                                )
                            },
                            {
                                "normalize", new Entry(
                                    "normalize", false,
                                    active: true,
                                    help:
                                    "(default = false) Whether normalization is applied to the vector observation inputs. This normalization is based on the running average and variance of the vector observation. Normalization can be helpful in cases with complex continuous control problems, but may be harmful with simpler discrete control problems."
                                )
                            },
                            {
                                "vis_encode_type", new Entry(
                                    "vis_encode_type", "simple",
                                    active: true,
                                    help:
                                    "(default = simple) Encoder type for encoding visual observations.\n\nsimple (default) uses a simple encoder which consists of two convolutional layers, nature_cnn uses the CNN implementation proposed by Mnih et al., consisting of three convolutional layers, and resnet uses the IMPALA Resnet consisting of three stacked layers, each with two residual blocks, making a much larger network than the other two. match3 is a smaller CNN (Gudmundsoon et al.) that is optimized for board games, and can be used down to visual observation sizes of 5x5."
                                )
                            },
                            // PPO- and MA-POCA-specific
                            {
                                "beta", new Entry(
                                    "beta", 0.005f,
                                    active: true,
                                    trainers: new List<string> {"ppo", "paco"},
                                    help:
                                    "(default = 5.0e-3) Strength of the entropy regularization, which makes the policy \"more random.\" This ensures that agents properly explore the action space during training. Increasing this will ensure more random actions are taken. This should be adjusted such that the entropy (measurable from TensorBoard) slowly decreases alongside increases in reward. If entropy drops too quickly, increase beta. If entropy drops too slowly, decrease beta.\n\nTypical range: 1e-4 - 1e-2"
                                )
                            },
                            {
                                "epsilon", new Entry(
                                    "epsilon", 0.2f,
                                    active: true,
                                    trainers: new List<string> {"ppo", "paco"},
                                    help:
                                    "(default = 0.2) Influences how rapidly the policy can evolve during training. Corresponds to the acceptable threshold of divergence between the old and new policies during gradient descent updating. Setting this value small will result in more stable updates, but will also slow the training process.\n\nTypical range: 0.1 - 0.3"
                                )
                            },
                            {
                                "lambd", new Entry(
                                    "lambd", 0.95f,
                                    active: true,
                                    trainers: new List<string> {"ppo", "paco"},
                                    help:
                                    "(default = 0.95) Regularization parameter (lambda) used when calculating the Generalized Advantage Estimate (GAE). This can be thought of as how much the agent relies on its current value estimate when calculating an updated value estimate. Low values correspond to relying more on the current value estimate (which can be high bias), and high values correspond to relying more on the actual rewards received in the environment (which can be high variance). The parameter provides a trade-off between the two, and the right value can lead to a more stable training process.\n\nTypical range: 0.9 - 0.95"
                                )
                            },
                            {
                                "num_epoch", new Entry(
                                    "num_epoch", 3,
                                    active: true,
                                    trainers: new List<string> {"ppo", "paco"},
                                    help:
                                    "(default = 3) Number of passes to make through the experience buffer when performing gradient descent optimization.The larger the batch_size, the larger it is acceptable to make this. Decreasing this will ensure more stable updates, at the cost of slower learning.\n\nTypical range: 1000 - 10000"
                                )
                            },
                            // SAC-specific
                            {
                                "buffer_init_steps", new Entry(
                                    "buffer_init_steps", 0,
                                    active: true,
                                    trainers: new List<string> {"sac"},
                                    help:
                                    "(default = 0) Number of experiences to collect into the buffer before updating the policy model. As the untrained policy is fairly random, pre-filling the buffer with random actions is useful for exploration. Typically, at least several episodes of experiences should be pre-filled.\n\nTypical range: 1000 - 10000"
                                )
                            },
                            {
                                "init_entcoef", new Entry(
                                    "init_entcoef", 1.0f,
                                    active: true,
                                    trainers: new List<string> {"sac"},
                                    help:
                                    "(default = 1.0) How much the agent should explore in the beginning of training. Corresponds to the initial entropy coefficient set at the beginning of training. In SAC, the agent is incentivized to make its actions entropic to facilitate better exploration. The entropy coefficient weighs the true reward with a bonus entropy reward. The entropy coefficient is automatically adjusted to a preset target entropy, so the init_entcoef only corresponds to the starting value of the entropy bonus. Increase init_entcoef to explore more in the beginning, decrease to converge to a solution faster.\n\nTypical range: (Continuous): 0.5 - 1.0; (Discrete): 0.05 - 0.5"
                                )
                            },
                            {
                                "save_replay_buffer", new Entry(
                                    "save_replay_buffer", false,
                                    active: true,
                                    trainers: new List<string> {"sac"},
                                    help:
                                    "(default = false) Whether to save and load the experience replay buffer as well as the model when quitting and re-starting training. This may help resumes go more smoothly, as the experiences collected won't be wiped. Note that replay buffers can be very large, and will take up a considerable amount of disk space. For that reason, we disable this feature by default."
                                )
                            },
                            {
                                "tau", new Entry(
                                    "tau", 0.005f,
                                    active: true,
                                    trainers: new List<string> {"sac"},
                                    help:
                                    "(default = 0.005) How aggressively to update the target network used for bootstrapping value estimation in SAC. Corresponds to the magnitude of the target Q update during the SAC model update. In SAC, there are two neural networks: the target and the policy. The target network is used to bootstrap the policy's estimate of the future rewards at a given state, and is fixed while the policy is being updated. This target is then slowly updated according to tau. Typically, this value should be left at 0.005. For simple problems, increasing tau to 0.01 might reduce the time it takes to learn, at the cost of stability.\n\nTypical range: 0.005 - 0.01"
                                )
                            },
                            {
                                "steps_per_update", new Entry(
                                    "steps_per_update", 1,
                                    active: true,
                                    trainers: new List<string> {"sac"},
                                    help:
                                    "(default = 1) Average ratio of agent steps (actions) taken to updates made of the agent's policy. In SAC, a single \"update\" corresponds to grabbing a batch of size batch_size from the experience replay buffer, and using this mini batch to update the models. Note that it is not guaranteed that after exactly steps_per_update steps an update will be made, only that the ratio will hold true over many steps. Typically, steps_per_update should be greater than or equal to 1. Note that setting steps_per_update lower will improve sample efficiency (reduce the number of steps required to train) but increase the CPU time spent performing updates. For most environments where steps are fairly fast (e.g. our example environments) steps_per_update equal to the number of agents in the scene is a good balance. For slow environments (steps take 0.1 seconds or more) reducing steps_per_update may improve training speed. We can also change steps_per_update to lower than 1 to update more often than once per step, though this will usually result in a slowdown unless the environment is very slow.\n\nTypical range: 1 - 20"
                                )
                            },
                            {
                                "reward_signal_num_update", new Entry(
                                    "reward_signal_num_update", 1,
                                    active: true,
                                    trainers: new List<string> {"sac"},
                                    help:
                                    "(default = steps_per_update) Number of steps per mini batch sampled and used for updating the reward signals. By default, we update the reward signals once every time the main policy is updated. However, to imitate the training procedure in certain imitation learning papers (e.g. Kostrikov et. al, Blondé et. al), we may want to update the reward signal (GAIL) M times for every update of the policy. We can change steps_per_update of SAC to N, as well as reward_signal_steps_per_update under reward_signals to N / M to accomplish this. By default, reward_signal_steps_per_update is set to steps_per_update."
                                )
                            }
                        }
                    )
                },
                // Reward Signals
                {
                    "extrinsic", new Entry("extrinsic",
                        help:
                        "Enable these settings to ensure that your training run incorporates your environment-based reward signal:",
                        active: false,
                        entries: new Entries
                        {
                            {
                                "strength", new Entry(
                                    "strength", 1.0f,
                                    active: true,
                                    help:
                                    "(default = 1.0) Factor by which to multiply the reward given by the environment. Typical ranges will vary depending on the reward signal.\n\nTypical range: 1.00"
                                )
                            },
                            {
                                "gamma", new Entry(
                                    "gamma", 0.99f,
                                    active: true,
                                    help:
                                    "(default = 0.99) Discount factor for future rewards coming from the environment. This can be thought of as how far into the future the agent should care about possible rewards. In situations when the agent should be acting in the present in order to prepare for rewards in the distant future, this value should be large. In cases when rewards are more immediate, it can be smaller. Must be strictly smaller than 1.\n\nTypical range: 0.8 - 0.995"
                                )
                            }
                        }
                    )
                },
                {
                    "curiosity", new Entry("curiosity",
                        help: "To enable curiosity, provide these settings:",
                        active: false,
                        entries: new Entries
                        {
                            {
                                "strength", new Entry(
                                    "strength", 1.0f,
                                    active: true,
                                    help:
                                    "(default = 1.0) Magnitude of the curiosity reward generated by the intrinsic curiosity module. This should be scaled in order to ensure it is large enough to not be overwhelmed by extrinsic reward signals in the environment. Likewise it should not be too large to overwhelm the extrinsic reward signal.\n\nTypical range: 0.001 - 0.1"
                                )
                            },
                            {
                                "gamma", new Entry(
                                    "gamma", 0.99f,
                                    active: true,
                                    help:
                                    "(default = 0.99) Discount factor for future rewards.\n\nTypical range: 0.8 - 0.995"
                                )
                            },
                            {
                                "network_settings", new Entry(
                                    "network_settings", 64,
                                    active: true,
                                    help:
                                    "Please see the documentation for network_settings under Common Trainer Configurations. The network specs used by the intrinsic curiosity model. The value should of hidden_units should be small enough to encourage the ICM to compress the original observation, but also not too small to prevent it from learning to differentiate between expected and actual observations.\n\nTypical range: 64 - 256"
                                )
                            },
                            {
                                "learning_rate", new Entry(
                                    "learning_rate", 0.0003f,
                                    active: true,
                                    help:
                                    "(default = 3e-4) Learning rate used to update the intrinsic curiosity module. This should typically be decreased if training is unstable, and the curiosity loss is unstable.\n\nTypical range: 1e-5 - 1e-3"
                                )
                            }
                        }
                    )
                }
                // Remaining:
                // https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Training-Configuration-File.md#gail-intrinsic-reward
            })
        },
        {
            "engine_settings", new Entry("engine_settings", new Entries
            {
                {"time_scale", new Entry("time_scale", 1.0f)},
                {"quality_level", new Entry("quality_level", 5)},
                {"target_frame_rate", new Entry("target_frame_rate", -1)},
                {"capture_frame_rate", new Entry("capture_frame_rate", 60)},
                {"no_graphics", new Entry("no_graphics", false)},
                {"width", new Entry("width", 84)},
                {"height", new Entry("height", 84)}
            })
        },
        // { "key", new Entry("environment_parameters", "null"),
        {
            "checkpoint_settings", new Entry("checkpoint_settings", new Entries
            {
                {"run_id", new Entry("run_id", "mlady")},
                {"initialize_from", new Entry("initialize_from", "")},
                {"load_model", new Entry("load_model", false)},
                {"resume", new Entry("resume", false)},
                {"force", new Entry("force", false)},
                {"train_model", new Entry("train_model", false)},
                {"inference", new Entry("inference", false)},
                {"results_dir", new Entry("results_dir", "Assets/ML-Agents/Results")}
            })
        },
        {
            "torch_settings", new Entry("torch_settings", new Entries
            {
                {"device", new Entry("device", "cuda")}
            })
        },
        {"debug", new Entry("debug", false)}
    };

    public string path;
    public bool fileExists => path != null && path != "" && File.Exists(path);

    public void WriteLines(Entries entries, StreamWriter writer, int indent = 0)
    {
        var indentation = new string(' ', indent * 2);
        foreach (var entry in entries.Values)
        {
            if (!entry.trainers.Contains(trainer) || entry.active == false) continue;
            if (entry.entries != null && entry.entries.Count > 0)
            {
                writer.WriteLine(indentation + entry.key + ":");
                WriteLines(entry.entries, writer, ++indent);
                --indent;
                continue;
            }

            switch (entry.value.GetType().ToString())
            {
                case "System.String":
                    if (
                        // Don't write empty strings to file
                        entry.value.GetType().ToString() == "System.String" &&
                        (string) entry.value == "" || entry.value == null
                    )
                    {
                        continue;
                    }
                    writer.WriteLine(indentation + entry.key + ": " + entry.value);
                    break;
                case "System.Int32":
                    writer.WriteLine(indentation + entry.key + ": " + entry.value);
                    break;
                case "System.Single":
                    writer.WriteLine(indentation + entry.key + ": " + entry.value);
                    break;
                case "System.Boolean":
                    writer.WriteLine(indentation + entry.key + ": " + entry.value.ToString().ToLower());
                    break;
            }
        }
    }

    public void WriteFile()
    {
        if (!fileExists) return;
        File.WriteAllText(path, string.Empty);
        var writer = new StreamWriter(path, true);
        WriteLines(entries, writer);
        writer.Close();
    }

    public string ReadFile()
    {
        if (!fileExists) return "No YAML-file available yet";
        var reader = new StreamReader(path);
        var yaml = reader.ReadToEnd();
        reader.Close();
        return yaml;
    }

    public class Entry
    {
        public bool active;
        public Entries entries;
        public string help;
        public string key;
        public string label;
        public List<string> trainers = new List<string> { "ppo", "sac", "poca" };
        public object value;

        // Data Type Entries
        // ----------------------------------------
        public Entry(
            string key,
            string input,
            string help = null,
            string label = null,
            bool active = true,
            List<string> trainers = null
        )
        {
            value = input;
            AssignCommonProperties(key, label, help, active, trainers);
        }

        public Entry(
            string key,
            bool input,
            string help = null,
            string label = null,
            bool active = true,
            List<string> trainers = null
        )
        {
            value = input;
            AssignCommonProperties(key, label, help, active, trainers);
        }

        public Entry(
            string key,
            int input,
            string help = null,
            string label = null,
            bool active = true,
            List<string> trainers = null
        )
        {
            value = input;
            AssignCommonProperties(key, label, help, active, trainers);
        }

        public Entry(
            string key,
            float input,
            string help = null,
            string label = null,
            bool active = true,
            List<string> trainers = null
        )
        {
            value = input;
            AssignCommonProperties(key, label, help, active, trainers);
        }
        // ----------------------------------------

        // Entry of Entries
        // ----------------------------------------
        public Entry(
            string key,
            Entries entries,
            string help = null,
            string label = null,
            bool active = true,
            List<string> trainers = null
        )
        {
            this.entries = entries;
            AssignCommonProperties(key, label, help, active, trainers);
        }
        // ----------------------------------------

        private void AssignCommonProperties(
            string key,
            string label = null,
            string help = null,
            bool active = true,
            List<string> trainers = null
        )
        {
            this.key = key;
            this.label = label != null ? label : key;
            this.help = help;
            this.active = active;
            if (trainers != null) this.trainers = trainers;
        }
    }
}
#endif