using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class Score : MonoBehaviour
{
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI scoreHistoryText;
    public TextMeshProUGUI stepText;
    public TextMeshProUGUI stepsPerSecondText;
    public TextMeshProUGUI speedKMHText;
    public TextMeshProUGUI speedMSText;
    public EnvironmentData env;
    public VehicleAgent agent;

    Colors colors = new Colors();
    class Colors
    {
        public Color32 success = new Color32(69, 255, 131, 255);
        public Color32 danger = new Color32(255, 25, 131, 255);
        public Color32 warning = new Color32(255, 255, 131, 255);
        public Color32 primary = new Color32(255, 255, 255, 255);
    };

    private float reward = 0;
    private List<float> rewardHistory = new List<float>();
    string toStringFormat = "F1";
    int oldStepTotal = 0;
    float oldStepTotalTime = 0;
    float syncPeriod = 0.2f;

    public int StepDelta => agent.StepTotal - oldStepTotal;

    void Start()
    {
        if (currentScoreText == null) currentScoreText = GetComponentInChildren<TextMeshProUGUI>();
        if (agent != null && currentScoreText != null)
        {
            StartCoroutine(SyncUI());
        }
        else if (currentScoreText != null)
        {
            currentScoreText.SetText("No agent");
            currentScoreText.color = colors.warning;
        }
    }

    IEnumerator SyncUI()
    {
        for (; ; )
        {
            stepText.SetText(
                agent.StepCount.ToString()
                + "/"
                + agent.MaxStep
            );

            stepsPerSecondText.SetText(
                "(" +
                (
                    env.Instances.Product() * StepDelta /
                    (Time.realtimeSinceStartup - oldStepTotalTime)
                ).ToString("F0")
                + " steps/s in total)"
            );
            oldStepTotalTime = Time.realtimeSinceStartup;
            oldStepTotal = agent.StepTotal;

            speedKMHText.SetText(
                (agent.Speed * 3.6f).ToString("F0")
                + "km/h"
            );
            speedMSText.SetText(
                "("
                + agent.Speed.ToString("F0")
                + "m/s)"
            );

            scoreHistoryText.SetText(string.Empty);

            // foreach (var score in agent.RewardHistory)
            for (int i = agent.RewardHistory.Count - 1; i >= 0; i--)
            {
                scoreHistoryText.SetText(
                    scoreHistoryText.text
                    + agent.RewardHistory[i].ToString(toStringFormat)
                    + "\n"
                );
            }

            reward = agent.GetCumulativeReward();
            currentScoreText.SetText(reward.ToString(toStringFormat));

            if (reward < -0.5)
            {
                currentScoreText.color = colors.danger;
            }
            else if (reward < -0.05)
            {
                currentScoreText.color = colors.warning;
            }
            else if (reward < 1)
            {
                currentScoreText.color = colors.primary;
            }
            else if (reward < 2)
            {
                currentScoreText.color = colors.success;
            }

            yield return new WaitForSeconds(syncPeriod);
        }
    }
}
