using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour {
  public List<Movable> movables;

  public class Movable {
    public Transform tf;
    public Rigidbody rb;
    public Bounds bs;
  }
}
