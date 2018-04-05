using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Вращение объектов колесом мыши
[RequireComponent(typeof(ObjectIdentity))]
public class InteractibleBlock : Interactible {

    private bool wasPickedUpBefore = false;

    void Update() {
        if (!isActive) return;

        var identity = GetComponent<ObjectIdentity>();
        var freeRotatingIdentity = GetComponent<FreeRotatingObjectIdentity>();
        if (!identity) return;

        // Изменение оси вращения по нажатию ctrl
        var ctrlDown = Input.GetKeyDown(KeyCode.LeftControl);
        if (freeRotatingIdentity && ctrlDown) {
            freeRotatingIdentity.NextRotationAxis();
        }

        // Изменение вращения по колесику мыши
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

    public override void StartInteract(Transform instigator) {
        base.StartInteract(instigator);
        var identity = GetComponent<ObjectIdentity>();

        // Загружаем поворот этого объекта из менеджера, если он не был поднят ранее
        if (!wasPickedUpBefore) {
            identity.UpdateRotationIndex();
            wasPickedUpBefore = true;
        }
        else {
            // либо сохраняем вращение в менеджер, если объект уже поднимался
            identity.SaveRotationIndex();
        }
    }
}
