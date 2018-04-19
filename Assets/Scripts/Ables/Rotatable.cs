using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Блок с указанным списком возможных поворотов
public abstract class Rotatable : MonoBehaviour {

    public float rotationAngle = 90f;
    public Vector3 debugAngleDisplay = new Vector3();

    protected virtual void Awake() {
    }

    protected virtual void Start() {
    }

    public virtual void IncreaseRotationIndex(int steps = 1) {
        if (!CanRotate()) return;
        SetRotationIndex(GetRotationIndex() + steps);
        WrapRotationIndex();
    }

    public virtual void DecreaseRotationIndex(int steps = 1) {
        if (!CanRotate()) return;
        SetRotationIndex(GetRotationIndex() - steps);
        WrapRotationIndex();
    }

    protected abstract void WrapRotationIndex();

    public abstract int GetRotationIndex(int axis = -1);

    public abstract void SetRotationIndex(int index);

    public abstract void UpdateRotationIndex();

    public abstract void SaveRotationIndex();

    public abstract bool CanRotate();

    public abstract void InitRotations();

    public abstract Quaternion GetRotation();
}
