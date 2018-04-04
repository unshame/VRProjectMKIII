using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Используется для различения блоков редактором
public abstract class ObjectIdentity : MonoBehaviour {

    public string typeName;
    public Vector3 offset = Vector3.zero;
    protected float rotationAngle = 90f;
    public Vector3 debugAngleDisplay = new Vector3();

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

    public abstract void CopyIdentity(GameObject obj);

    public abstract bool CanRotate();

    public abstract Quaternion GetRotation();
}
