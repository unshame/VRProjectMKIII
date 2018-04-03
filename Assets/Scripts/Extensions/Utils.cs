using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorUtils {

	public static Vector3 DivideVectors(Vector3 a, Vector3 b) {
        return new Vector3(
            a.x / b.x,
            a.y / b.y,
            a.z / b.z
        );
    }

    public static Vector3i DivideVectorsFloorToInt(Vector3 a, Vector3 b) {
        return new Vector3i(
            Mathf.FloorToInt(a.x / b.x),
            Mathf.FloorToInt(a.y / b.y),
            Mathf.FloorToInt(a.z / b.z)
        );
    }

    public static Vector3i DivideVectorsCeilToInt(Vector3 a, Vector3 b) {
        return new Vector3i(
            Mathf.CeilToInt(a.x / b.x),
            Mathf.CeilToInt(a.y / b.y),
            Mathf.CeilToInt(a.z / b.z)
        );
    }

    public static Vector3i DivideVectorsRoundToInt(Vector3 a, Vector3 b) {
        return new Vector3i(
            Mathf.RoundToInt(a.x / b.x),
            Mathf.RoundToInt(a.y / b.y),
            Mathf.RoundToInt(a.z / b.z)
        );
    }
}
