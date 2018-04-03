using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Вращение объектов колесом мыши
public class InteractibleBlock : Interactible {

    void Update() {
        if (!isActive) return;

        var identity = GetComponent<ObjectIdentity>();
        if (!identity) return;

        var ctrlDown = Input.GetKeyDown(KeyCode.LeftControl);
        if (ctrlDown) {
            identity.NextRotationAxis();
        }

        var direction = Input.GetAxis("Mouse ScrollWheel");
        if (direction == 0) return;

        var abs = (int)Mathf.Abs(Mathf.Round(direction * 10));

        if (direction > 0) {
            identity.IncreaseRotationIndex(abs);
        }
        else {
            identity.DecreaseRotationIndex(abs);
        }
    }
}
