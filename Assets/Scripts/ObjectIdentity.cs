using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Используется для различения блоков редактором
public class ObjectIdentity : MonoBehaviour {

    private static int NUM_AXIS = 3;

    public string typeName;
    public Vector3 offset = Vector3.zero;
    public bool xAxisRotation = false;
    public bool yAxisRotation = false;
    public bool zAxisRotation = false;
    public int rotationAxis;
    [HideInInspector]
    public float rotationAngle = 90f;

    public bool usePredefinedRotations = false;
    public List<Vector3> predefinedRotations = new List<Vector3>();
    public float[] debugAngleDisplay = new float[NUM_AXIS];


    int[] rotationIndexes = new int[NUM_AXIS];
    bool[] rotationAllowed = new bool[NUM_AXIS];

    void Start() {
        if (!CanRotate()) return;
        if (rotationAxis < 0 || rotationAxis >= NUM_AXIS || !rotationAllowed[rotationAxis]) {
            NextRotationAxis();
        }
    }

    public void IncreaseRotationIndex(int steps = 1) {
        if (!CanRotate()) return;
        SetRotationIndex(GetRotationIndex() + steps);
        WrapRotationIndex();
    }

    public void DecreaseRotationIndex(int steps = 1) {
        if (!CanRotate()) return;
        SetRotationIndex(GetRotationIndex() - steps);
        WrapRotationIndex();
    }

    void WrapRotationIndex() {
        if (Mathf.Abs(Mathf.FloorToInt(rotationAngle * GetRotationIndex())) % 360 == 0) {
            SetRotationIndex(0);
        }
    }

    public int GetRotationIndex(int axis = -1) {
        if (!CanRotate()) return 0;
        if (axis == -1) axis = rotationAxis;
        return rotationIndexes[axis];
    }

    public void SetRotationIndex(int index) {
        if (!CanRotate()) return;
        rotationIndexes[rotationAxis] = index;
    }

    public void NextRotationAxis() {
        if (!CanRotate()) return;
        rotationAxis++;
        if (rotationAxis >= NUM_AXIS) rotationAxis = 0;
        if (!rotationAllowed[rotationAxis]) NextRotationAxis();
    }
    public void PrevRotationAxis() {
        if (!CanRotate()) return;
        rotationAxis--;
        if (rotationAxis <= 0) rotationAxis = NUM_AXIS;
        if (!rotationAllowed[rotationAxis]) PrevRotationAxis();
    }

    public bool CanRotate() {
        for(int i = 0; i < NUM_AXIS; i++) {
            debugAngleDisplay[i] = rotationIndexes[i] * rotationAngle;
        }
        rotationAllowed[0] = xAxisRotation;
        rotationAllowed[1] = yAxisRotation;
        rotationAllowed[2] = zAxisRotation;
        return xAxisRotation || yAxisRotation || zAxisRotation;
    }
}
