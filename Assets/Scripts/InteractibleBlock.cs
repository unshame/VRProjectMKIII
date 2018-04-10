using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Вращение объектов колесом мыши
[RequireComponent(typeof(ObjectIdentity))]
public class InteractibleBlock : Interactible {

    private bool wasPickedUpBefore = false;
    private bool wasPutDownBefore = false;
    public bool alwaysUpdateRotation = true;
    public Spawner spawner;

    void Update() {
        if (!isActive) return;

        var identity = GetComponent<ObjectIdentity>();
        var freeRotatingIdentity = GetComponent<FreeRotatingObjectIdentity>();
        if (!identity) return;

        var shouldUpdateRotation = false;

        // Изменение оси вращения по нажатию ctrl
        var ctrlDown = Input.GetKeyDown(KeyCode.LeftControl);
        if (freeRotatingIdentity && ctrlDown) {
            freeRotatingIdentity.NextRotationAxis();
            shouldUpdateRotation = true;
        }

        // Изменение вращения по колесику мыши
        var direction = Input.GetAxis("Mouse ScrollWheel");
        if (direction != 0) {

            var abs = (int)Mathf.Abs(Mathf.Round(direction * 10));

            if (direction > 0) {
                identity.IncreaseRotationIndex(abs);
            }
            else {
                identity.DecreaseRotationIndex(abs);
            }
            shouldUpdateRotation = true;
        }

        if (shouldUpdateRotation || alwaysUpdateRotation) {
            transform.parent.rotation = RotationManager.MainBuildStation.transform.rotation * identity.GetRotation();
        }
    }

    public override void StartInteract(Transform instigator) {
        base.StartInteract(instigator);
        var identity = GetComponent<ObjectIdentity>();

        // Загружаем поворот этого объекта из менеджера, если он не был поднят ранее
        if (!wasPickedUpBefore) {
            identity.UpdateRotationIndex();
            wasPickedUpBefore = true;
            if (spawner) {
                spawner.SpawningAllowed = false;
            }
        }
        else {
            // либо сохраняем вращение в менеджер, если объект уже поднимался
            identity.SaveRotationIndex();
        }
    }

    public override void StopInteract() {
        base.StopInteract();

        if (!wasPutDownBefore) {
            if (spawner) {
                spawner.SpawningAllowed = true;
            }
            wasPutDownBefore = true;
        }
    }
}
