using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorUtils {

	public static Vector3 Divide(Vector3 a, Vector3 b) {
        return new Vector3(
            a.x / b.x,
            a.y / b.y,
            a.z / b.z
        );
    }

    public static Vector3i FloorToInt(Vector3 a) {
        return new Vector3i(
            Mathf.FloorToInt(a.x),
            Mathf.FloorToInt(a.y),
            Mathf.FloorToInt(a.z)
        );
    }

    public static Vector3i CeilToInt(Vector3 a) {
        return new Vector3i(
            Mathf.CeilToInt(a.x),
            Mathf.CeilToInt(a.y),
            Mathf.CeilToInt(a.z)
        );
    }

    public static Vector3i RoundToInt(Vector3 a) {
        return new Vector3i(
            Mathf.RoundToInt(a.x),
            Mathf.RoundToInt(a.y),
            Mathf.RoundToInt(a.z)
        );
    }

    public static Vector3i RoundAroundToInt(Vector3 a, float breakpoint) {
        return new Vector3i(
            (a.x % 1) > breakpoint ? Mathf.CeilToInt(a.x) : Mathf.FloorToInt(a.x),
            (a.y % 1) > breakpoint ? Mathf.CeilToInt(a.y) : Mathf.FloorToInt(a.y),
            (a.z % 1) > breakpoint ? Mathf.CeilToInt(a.z) : Mathf.FloorToInt(a.z)
        );
    }

    public static Vector3i Min(Vector3i a, Vector3i b) {
        return new Vector3i(
            Mathf.Min(a.x, b.x),
            Mathf.Min(a.y, b.y),
            Mathf.Min(a.z, b.z)
        );
    }

    public static Vector3i Max(Vector3i a, Vector3i b) {
        return new Vector3i(
            Mathf.Max(a.x, b.x),
            Mathf.Max(a.y, b.y),
            Mathf.Max(a.z, b.z)
        );
    }
}
