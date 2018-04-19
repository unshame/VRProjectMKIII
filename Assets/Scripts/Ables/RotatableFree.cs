using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Компонент свободного поворота объекта (deprecated)
public class RotatableFree : Rotatable {

    private static int NUM_AXIS = 3;

    public bool xAxisRotation = false;
    public bool yAxisRotation = false;
    public bool zAxisRotation = false;
    public int rotationAxis;

    int[] rotationIndexes = new int[NUM_AXIS];
    bool[] rotationAllowed = new bool[NUM_AXIS];

    protected override void Awake() {
        if (!CanRotate()) return;
        WrapRotationIndex();
        if (!rotationAllowed[rotationAxis]) {
            NextRotationAxis();
        }
    }

    protected override void WrapRotationIndex() {
        var rotationIndex = GetRotationIndex();
        if (Mathf.Abs(Mathf.FloorToInt(rotationAngle * rotationIndex)) % 360 == 0) {
            SetRotationIndex(0);
        }
    }

    public override int GetRotationIndex(int axis = -1) {
        if (!CanRotate()) return 0;
        if (axis == -1) axis = rotationAxis;
        return rotationIndexes[axis];
    }

    public override void SetRotationIndex(int index) {
        if (!CanRotate()) return;
        rotationIndexes[rotationAxis] = index;
    }

    public override void LoadRotationIndex() {
    }

    public override void SaveRotationIndex() {
    }

    public void NextRotationAxis() {
        rotationAxis++;
        if (rotationAxis >= NUM_AXIS) rotationAxis = 0;
        if (!rotationAllowed[rotationAxis]) NextRotationAxis();
    }

    public void PrevRotationAxis() {
        rotationAxis--;
        if (rotationAxis <= 0) rotationAxis = NUM_AXIS;
        if (!rotationAllowed[rotationAxis]) PrevRotationAxis();
    }

    public override bool CanRotate() {
        for (int i = 0; i < NUM_AXIS; i++) {
            debugAngleDisplay[i] = rotationIndexes[i] * rotationAngle;
        }
        rotationAllowed[0] = xAxisRotation;
        rotationAllowed[1] = yAxisRotation;
        rotationAllowed[2] = zAxisRotation;
        return xAxisRotation || yAxisRotation || zAxisRotation;
    }

    public override Quaternion GetRotation() {
        if (!CanRotate()) return Quaternion.identity;

        var rotationIndex = GetRotationIndex();
        if (rotationAxis == 0) {
            return Quaternion.Euler(rotationAngle * rotationIndex, 0, 0);
        }
        if (rotationAxis == 1) {
            return Quaternion.Euler(0, rotationAngle * rotationIndex, 0);
        }
        if (rotationAxis == 2) {
            return Quaternion.Euler(0, 0, rotationAngle * rotationIndex);
        }
        return Quaternion.identity;
    }

}
