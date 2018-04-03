using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Используется для различения блоков редактором
public class ObjectIdentity : MonoBehaviour {

    public enum RotationTypes { Nothing, X, Y, Z}

    public string TypeName;
    public Vector3 Offset = Vector3.zero;
    public RotationTypes RotateBy;
}
