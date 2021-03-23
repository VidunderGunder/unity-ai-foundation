using UnityEngine;
using TMPro;

public class UpdateText : MonoBehaviour
{
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI previousCumulativeRewardText;
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI rewardThisDifficultyText;
    [SerializeField] private VehicleAgent vehicleAgent;
    [SerializeField] private EnvironmentData env;
    private float rewardThisDifficulty;


    void Start()
    {
        Events.current.onUpdateSingelAgentRewardTriggerEnter += UpdateSingelTextMesh;
        Events.current.onUpdateAgentRewardTriggerEnter += UpdateTextMesh;
    }

    private void UpdateSingelTextMesh()
    {
        rewardText.text = string.Format("{0:N2}", vehicleAgent.GetCumulativeReward());
        previousCumulativeRewardText.text = string.Format("{0:N2}", vehicleAgent.PreviousCumulativeReward);
        difficultyText.text = string.Format("{0:N2}", env.Difficulty);
    }

    private void UpdateTextMesh(float reward)
    {
        rewardThisDifficulty += reward;
        if (env.CumulativeRewardToNextDifficulty <= rewardThisDifficulty && env.Difficulty < 1)
        {
            env.SetDifficulty(env.Difficulty + env.NextDifficulty);
            rewardThisDifficulty = 0;
        }
        difficultyText.text = string.Format("{0:N2}", env.Difficulty);
        rewardThisDifficultyText.text = string.Format("{0:N2}", rewardThisDifficulty);
    }
}

