using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Face : MonoBehaviour {
  public Mood mood;
  public enum Mood {
    Happy,
    Neutral,
    Sad,
    Shocked,
    Asleep,
  }
  [Space]
  [Header("Eyes")]
  [SerializeField] private GameObject eyesOpen;
  [SerializeField] private GameObject eyesClosed;
  [Space]
  [Header("Mouth")]
  [SerializeField] private GameObject mouthHappy;
  [SerializeField] private GameObject mouthNeutral;
  [SerializeField] private GameObject mouthSad;
  [SerializeField] private GameObject mouthOpen;

  [Range(0, 10f)] [SerializeField] private WaitForSeconds eyesOpenDuration = new WaitForSeconds(1f);
  [Range(0, 1f)] [SerializeField] private WaitForSeconds blinkDuration = new WaitForSeconds(0.1f);

  void Awake() {

  }

  void Start() {
    StartCoroutine("Blink");
    SetMood();
  }

  void Update() {
    SetMood();
  }

  public void SetMood() {
    mouthHappy.SetActive(false);
    mouthNeutral.SetActive(false);
    mouthSad.SetActive(false);
    mouthOpen.SetActive(false);

    switch (mood) {
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

  void OpenEyes() {
    eyesOpen.SetActive(true);
    eyesClosed.SetActive(false);
  }

  void CloseEyes() {
    eyesOpen.SetActive(false);
    eyesClosed.SetActive(true);
  }

  IEnumerator Blink() {
    for (; ; ) {
      if (mood != Mood.Asleep) OpenEyes();
      yield return new WaitForSeconds(Random.Range(0.75f, 2.5f));
      if (mood != Mood.Asleep) CloseEyes();
      yield return new WaitForSeconds(Random.Range(0.1f, 0.03f));
    }
  }
}
