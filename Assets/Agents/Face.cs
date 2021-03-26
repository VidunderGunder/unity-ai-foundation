using System.Collections;
using UnityEngine;

[ExecuteAlways]
public class Face : MonoBehaviour
{
    public enum Mood
    {
        Happy,
        Neutral,
        Sad,
        Shocked,
        Asleep
    }

    [Range(0, 1f)] [SerializeField] private WaitForSeconds blinkDuration = new WaitForSeconds(0.1f);
    [SerializeField] private GameObject eyesClosed;

    [Header("Eyes")] [SerializeField] private GameObject eyesOpen;

    [Range(0, 10f)] [SerializeField] private WaitForSeconds eyesOpenDuration = new WaitForSeconds(1f);
    public Mood mood;

    [Header("Mouth")] [SerializeField] private GameObject mouthHappy;

    [SerializeField] private GameObject mouthNeutral;
    [SerializeField] private GameObject mouthOpen;
    [SerializeField] private GameObject mouthSad;

    private void Start()
    {
        StartCoroutine(Blink());
        SetMood();
    }

    private void Update()
    {
        SetMood();
    }

    public void SetMood()
    {
        mouthHappy.SetActive(false);
        mouthNeutral.SetActive(false);
        mouthSad.SetActive(false);
        mouthOpen.SetActive(false);

        switch (mood)
        {
            case Mood.Happy:
                mouthHappy.SetActive(true);
                break;
            case Mood.Neutral:
                mouthNeutral.SetActive(true);
                break;
            case Mood.Sad:
                mouthSad.SetActive(true);
                break;
            case Mood.Shocked:
                mouthOpen.SetActive(true);
                break;
            case Mood.Asleep:
                CloseEyes();
                break;
        }
    }

    private void OpenEyes()
    {
        eyesOpen.SetActive(true);
        eyesClosed.SetActive(false);
    }

    private void CloseEyes()
    {
        eyesOpen.SetActive(false);
        eyesClosed.SetActive(true);
    }

    private IEnumerator Blink()
    {
        for (; ; )
        {
            if (mood != Mood.Asleep) OpenEyes();
            yield return new WaitForSeconds(Random.Range(0.75f, 2.5f));
            if (mood != Mood.Asleep) CloseEyes();
            yield return new WaitForSeconds(Random.Range(0.1f, 0.03f));
        }
    }
}