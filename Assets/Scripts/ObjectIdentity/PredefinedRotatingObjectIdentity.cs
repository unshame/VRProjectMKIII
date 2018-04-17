using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Блок с указанным списком возможных поворотов
public class PredefinedRotatingObjectIdentity : ObjectIdentity {

    public List<Vector3> predefinedRotations = new List<Vector3>();
    public int predefinedRotationIndex = 0;

    protected override void Awake() {
        if (!CanRotate()) return;
        UpdateRotationIndex();
    }

    protected override void WrapRotationIndex() {
        if (predefinedRotationIndex < 0) {
            SetRotationIndex(predefinedRotations.Count - 1);
        }
        else if (predefinedRotationIndex >= predefinedRotations.Count) {
            SetRotationIndex(0);
        }

    }

    public override int GetRotationIndex(int axis = -1) {
        if (!CanRotate()) return 0;
        return predefinedRotationIndex;
    }

    public override void SetRotationIndex(int index) {
        if (!CanRotate()) return;
        predefinedRotationIndex = index;
        RotationManager.SetRotation(typeName, index);
        if (index >= 0 && index < predefinedRotations.Count) {
            debugAngleDisplay = predefinedRotations[index];
        }
        else {
            WrapRotationIndex();
        }
    }

    public override void UpdateRotationIndex() {
        var rotationIndex = RotationManager.GetRotation(typeName);
        if (rotationIndex == -1) {
            rotationIndex = predefinedRotationIndex;
        }
        SetRotationIndex(rotationIndex);
    }

    public override void SaveRotationIndex() {
        RotationManager.SetRotation(typeName, predefinedRotationIndex);
    }

    public override void CopyIdentity(GameObject obj) {
        var identity = obj.GetComponent<ObjectIdentity>();
        if (!identity) return;
        SetRotationIndex(identity.GetRotationIndex());
    }


    public override bool CanRotate() {
        return predefinedRotations.Count > 0;
    }

    public override Quaternion GetRotation() {
        if (!CanRotate()) return Quaternion.identity;

        var rotation = predefinedRotations[predefinedRotationIndex];
        return Quaternion.Euler(rotation.x, rotation.y, rotation.z);
    }

    public override void InitRotations() {
    }

}
