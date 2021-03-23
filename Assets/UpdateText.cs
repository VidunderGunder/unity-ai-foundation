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
        difficultyText.text = string.Format("{0:N2}", env.difficulty);
    }

    private void UpdateTextMesh(float reward)
    {
        rewardThisDifficulty += reward;
        if (env.cumulativeRewardToNextDifficulty <= rewardThisDifficulty && env.difficulty < 1)
        {
            env.difficulty += env.nextDifficulty;
            if (1 < env.difficulty) env.difficulty = 1;
            rewardThisDifficulty = 0;
        }
        difficultyText.text = string.Format("{0:N2}", env.difficulty);
        rewardThisDifficultyText.text = string.Format("{0:N2}", rewardThisDifficulty);
    }
}

