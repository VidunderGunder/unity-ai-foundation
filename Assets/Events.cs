using UnityEngine;

public class Events : MonoBehaviour
{
    public static Events current;

    private void Awake()
    {
        current = this;
    }
    public event System.Action onUpdateSingelAgentRewardTriggerEnter;
    public void UpdateSingelAgentRewardTriggerEnter()
    {
        if (onUpdateSingelAgentRewardTriggerEnter != null)
        {
            onUpdateSingelAgentRewardTriggerEnter();
        }
    }
    public event System.Action<float> onUpdateAgentRewardTriggerEnter;
    public void UpdateAgentsRewardTriggerEnter(float reward)
    {
        if (onUpdateAgentRewardTriggerEnter != null)
        {
            onUpdateAgentRewardTriggerEnter(reward);
        }
    }
}
