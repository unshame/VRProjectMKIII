using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Компонент поворота объектов
// Хранит, изменяет, сохраняет и загружает индекс поворота и возможные повороты, но не устанавливает поворот объекту самостоятельно
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

    public abstract void LoadRotationIndex();

    public abstract void SaveRotationIndex();

    public abstract bool CanRotate();

    public abstract Quaternion GetRotation();
}
