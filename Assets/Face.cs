using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : MonoBehaviour {
  [SerializeField] private GameObject eyesOpen;
  [SerializeField] private GameObject eyesClosed;
  [SerializeField] private GameObject mouthHappy;
  [SerializeField] private GameObject mouthNeutral;
  [SerializeField] private GameObject mouthSad;

  [Range(0, 10f)] [SerializeField] private WaitForSeconds eyesOpenDuration = new WaitForSeconds(1f);
  [Range(0, 1f)] [SerializeField] private WaitForSeconds blinkDuration = new WaitForSeconds(0.1f);

  void Awake() {
    eyesOpen.SetActive(true);
    eyesClosed.SetActive(false);

    mouthHappy.SetActive(false);
    mouthNeutral.SetActive(true);
    mouthSad.SetActive(false);

  }

  void Start() {
    StartCoroutine("Blink");
  }

  void Update() {

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
      OpenEyes();
      yield return new WaitForSeconds(Random.Range(0.5f, 2f));
      CloseEyes();
      yield return new WaitForSeconds(Random.Range(0.1f, 0.025f));
    }
  }
}
