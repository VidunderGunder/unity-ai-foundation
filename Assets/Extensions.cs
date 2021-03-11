using UnityEngine;

public static class TransformExtensions {
  public static void Reset(this Transform t) {
    t.position = Vector3.zero;
    t.localRotation = Quaternion.identity;
    t.localScale = new Vector3(1, 1, 1);
  }

  /// <summary>Finds relative vector pointing from self (local origin) to target.</summary>
  public static Vector3 RelativeVectorTo(this Transform self, Vector3 target) {
    return (Quaternion.Inverse(self.rotation) * (target - self.position));
  }
}

public static class RigidbodyExtensions {
  public static void Stop(this Rigidbody rb) {
    rb.velocity = Vector3.zero;
    rb.angularVelocity = Vector3.zero;
  }
}

public static class VectorExtensions {
  /// <summary>Returns the y-value of a given vector to 0.</summary>
  public static Vector3 Flattened(this Vector3 v) {
    return new Vector3(v.x, 0f, v.z);
  }
  /// <summary>Sets the y-value of a given vector to 0.</summary>
  public static void Flatten(this Vector3 v) {
    v = Flattened(v);
  }

  /// <summary>Finds distance between two flattened points.</summary>
  public static float FlattenedDistance(this Vector3 origin, Vector3 destination) {
    return Vector3.Distance(origin.Flattened(), destination.Flattened());
  }

  /// <summary>Removes z from a 3D-vector and returns a 2D-vector.</summary>
  public static Vector2 xy(this Vector3 v) { return new Vector2(v.x, v.y); }
  /// <summary>Removes y from a 3D-vector and returns a 2D-vector.</summary>
  public static Vector2 xz(this Vector3 v) { return new Vector2(v.x, v.z); }
  /// <summary>Removes x from a 3D-vector and returns a 2D-vector.</summary>
  public static Vector2 yz(this Vector3 v) { return new Vector2(v.y, v.z); }

  /// <summary>
  /// Turns 2D-vector to 3D-vector by shoving the ol' y to z, 
  /// and injecting a spanking new y in its place. Like so:
  /// <br />
  /// [x, y] -> [x, new y, old y]
  /// </summary>
  public static Vector3 UnflattenedGround(this Vector2 v, float y) {
    return new Vector3(v.x, y, v.y);
  }
}

public static class QuaternionExtensions {
  /// <summary>Spin the wheel!<br />Or...<br />Sphere, I guess?</summary>
  public static void Randomize(this Quaternion r) {
    r = Quaternion.Euler(
      Random.Range(-180f, 180f),
      Random.Range(-180f, 180f),
      Random.Range(-180f, 180f)
    );
  }
  /// <summary>
  /// Randomizes x-, y- and z-rotation from the starting point.<br />
  /// Meaningful range is 0-180.<br />
  /// x: (-max) - - - - - - (origin) - - - - - - (max)<br />
  /// y: (-max) - - - - - - (origin) - - - - - - (max)<br />
  /// z: (-max) - - - - - - (origin) - - - - - - (max)<br />
  /// </summary>
  public static void Randomize(this Quaternion r, float maxRotation) {
    r *= Quaternion.Euler(
      Random.Range(-maxRotation, maxRotation),
      Random.Range(-maxRotation, maxRotation),
      Random.Range(-maxRotation, maxRotation)
    );
  }
  /// <summary>
  /// Randomizes x-, y- and z-rotation from the starting point.<br />
  /// Meaningful range is 0-180.<br />
  /// x: (-maxX) - - - - - - (origin) - - - - - - (maxX)<br />
  /// y: (-maxY) - - - - - - (origin) - - - - - - (maxY)<br />
  /// z: (-maxZ) - - - - - - (origin) - - - - - - (maxZ)<br />
  /// </summary>
  public static void Randomize(this Quaternion r, Vector3 maxRotation) {
    r *= Quaternion.Euler(
      Random.Range(-maxRotation.x, maxRotation.x),
      Random.Range(-maxRotation.y, maxRotation.y),
      Random.Range(-maxRotation.z, maxRotation.z)
    );
  }
}