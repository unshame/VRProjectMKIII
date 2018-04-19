using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Компонент поворота объекта с указанным списком возможных поворотов
public class RotatablePredefined : Rotatable {

    public List<Vector3> predefinedRotations = new List<Vector3>();
    public int predefinedRotationIndex = 0;

    protected override void Awake() {
        if (!CanRotate()) return;
        LoadRotationIndex();
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
        var typeName = GetComponent<ObjectIdentity>().typeName;
        PropertyManager.SetRotation(typeName, index);
        if (index >= 0 && index < predefinedRotations.Count) {
            debugAngleDisplay = predefinedRotations[index];
        }
        else {
            WrapRotationIndex();
        }
    }

    public override void LoadRotationIndex() {
        var typeName = GetComponent<ObjectIdentity>().typeName;
        var rotationIndex = PropertyManager.GetRotation(typeName);
        if (rotationIndex == -1) {
            rotationIndex = predefinedRotationIndex;
        }
        SetRotationIndex(rotationIndex);
    }

    public override void SaveRotationIndex() {
        var typeName = GetComponent<ObjectIdentity>().typeName;
        PropertyManager.SetRotation(typeName, predefinedRotationIndex);
    }

    public override bool CanRotate() {
        return predefinedRotations.Count > 0;
    }

    public override Quaternion GetRotation() {
        if (!CanRotate()) return Quaternion.identity;

        var rotation = predefinedRotations[predefinedRotationIndex];
        return Quaternion.Euler(rotation.x, rotation.y, rotation.z);
    }

}
