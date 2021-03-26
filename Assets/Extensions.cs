using System.Collections.Generic;
using UnityEngine;

public static class StringExtensions
{
    public static string ToDirtyTitleCase(this string text)
    {
        var words = text.Split('_');
        text = "";
        var allLower = new List<string>
        {
            "of",
            "the",
            "in",
            "and",
            "to",
            "per"
        };
        var allUpper = new List<string>
        {
            "ID",
            "YAML"
        };
        var isFirst = true;

        foreach (var word in words)
        {
            if (isFirst)
                isFirst = false;
            else
                text += " ";

            if (word.Length <= 0) continue;

            if (allUpper.Contains(word.ToUpper()))
                text += word.ToUpper();
            else if (allLower.Contains(word.ToLower()))
                text += word.ToLower();
            else
                text += word[0].ToString().ToUpper() + word.Substring(1);
        }

        return text;
    }
}

public static class TransformExtensions
{
    public static void ResetLocal(this Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = new Vector3(1, 1, 1);
    }

    /// <summary>Finds relative vector pointing from self (local origin) to target.</summary>
    public static Vector3 RelativeVectorTo(this Transform self, Vector3 target)
    {
        return Quaternion.Inverse(self.rotation) * (target - self.position);
    }

    /// <summary>Finds relative vector pointing from self (local origin) to target.</summary>
    public static Vector3 PlaceOnGround(this Transform self, LayerMask ground, float offset = 0)
    {
        float radius;

        if (self.GetComponent<Collider>() != null)
            radius = self.GetComponent<Collider>().bounds.extents.y;
        else
            radius = 1f;

        RaycastHit hit;

        if (
            Physics.Raycast(
                new Ray(self.position - Vector3.down * radius * 1.01f, Vector3.down),
                out hit,
                Mathf.Infinity,
                ground
            )
        )
            if (hit.collider != null)
            {
                self.position = new Vector3(self.position.x, hit.point.y + radius + offset, self.position.z);
            }

        return self.position;
    }
}

public static class RigidbodyExtensions
{
    public static void Stop(this Rigidbody rb)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}

public static class VectorExtensions
{
    /// <summary>Returns the sum of all vector elements.</summary>
    public static float Sum(this Vector3 v)
    {
        return v.x + v.y + v.z;
    }

    /// <summary>Returns the sum of all vector elements.</summary>
    public static int Sum(this Vector3Int v)
    {
        return v.x + v.y + v.z;
    }

    /// <summary>Returns the product of all vector elements.</summary>
    public static float Product(this Vector3 v)
    {
        return v.x * v.y * v.z;
    }

    /// <summary>Returns the product of all vector elements.</summary>
    public static int Product(this Vector3Int v)
    {
        return v.x * v.y * v.z;
    }

    /// <summary>Returns the y-value of a given vector to 0.</summary>
    public static Vector3 Flattened(this Vector3 v)
    {
        return new Vector3(v.x, 0f, v.z);
    }

    /// <summary>Sets the y-value of a given vector to 0.</summary>
    public static void Flatten(this Vector3 v)
    {
        v = Flattened(v);
    }

    /// <summary>Finds distance between two flattened points.</summary>
    public static float FlattenedDistance(this Vector3 origin, Vector3 destination)
    {
        return Vector3.Distance(origin.Flattened(), destination.Flattened());
    }

    /// <summary>Removes z from a 3D-vector and returns a 2D-vector.</summary>
    public static Vector2 xy(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    /// <summary>Removes y from a 3D-vector and returns a 2D-vector.</summary>
    public static Vector2 xz(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    /// <summary>Removes x from a 3D-vector and returns a 2D-vector.</summary>
    public static Vector2 yz(this Vector3 v)
    {
        return new Vector2(v.y, v.z);
    }

    /// <summary>
    ///     Turns 2D-vector to 3D-vector by shoving the ol' y to z,
    ///     and injecting a spanking new y in its place. Like so:
    ///     <br />
    ///     [x, y] -> [x, new y, old y]
    /// </summary>
    public static Vector3 UnflattenedGround(this Vector2 v, float y)
    {
        return new Vector3(v.x, y, v.y);
    }
}

public static class QuaternionExtensions
{
    /// <summary>Spin the wheel!<br />Or...<br />Sphere, I guess?</summary>
    public static Quaternion Randomize(this Quaternion r)
    {
        r = Quaternion.Euler(
            Random.Range(-180f, 180f),
            Random.Range(-180f, 180f),
            Random.Range(-180f, 180f)
        );
        return r;
    }

    /// <summary>
    ///     Randomizes x-, y- and z-rotation from the starting point.<br />
    ///     Meaningful range is 0-180.<br />
    ///     x: (-max) - - - - - - (origin) - - - - - - (max)<br />
    ///     y: (-max) - - - - - - (origin) - - - - - - (max)<br />
    ///     z: (-max) - - - - - - (origin) - - - - - - (max)<br />
    /// </summary>
    public static Quaternion Randomize(this Quaternion r, float maxRotation)
    {
        r *= Quaternion.Euler(
            Random.Range(-maxRotation, maxRotation),
            Random.Range(-maxRotation, maxRotation),
            Random.Range(-maxRotation, maxRotation)
        );
        return r;
    }

    /// <summary>
    ///     Randomizes x-, y- and z-rotation from the starting point.<br />
    ///     Meaningful range is 0-180.<br />
    ///     x: (-maxX) - - - - - - (origin) - - - - - - (maxX)<br />
    ///     y: (-maxY) - - - - - - (origin) - - - - - - (maxY)<br />
    ///     z: (-maxZ) - - - - - - (origin) - - - - - - (maxZ)<br />
    /// </summary>
    public static Quaternion Randomize(this Quaternion r, Vector3 maxRotation)
    {
        r *= Quaternion.Euler(
            Random.Range(-maxRotation.x, maxRotation.x),
            Random.Range(-maxRotation.y, maxRotation.y),
            Random.Range(-maxRotation.z, maxRotation.z)
        );
        return r;
    }
}