using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Вращение объектов колесом мыши
[RequireComponent(typeof(ObjectIdentity))]
public class InteractableBlock : Interactable {

    private bool wasPickedUpBefore = false;
    private bool wasPutDownBefore = false;
    public bool alwaysUpdateRotation = true;
    public Spawner spawner;

    void Update() {
        if (!isActive) return;

        var direction = Input.GetAxis("Mouse ScrollWheel");
        var shouldUpdateRotation = alwaysUpdateRotation;

        var rotatingComponent = GetComponent<Rotatable>();
        var resizeableComponent = GetComponent<Resizable>();

        var ctrlDown = Input.GetKeyDown(KeyCode.LeftControl);
        var altPressed = Input.GetKey(KeyCode.LeftAlt);

        var controller = SteamVR_Controller.Input(0);

        if (direction != 0) {

            var abs = (int)Mathf.Abs(Mathf.Round(direction * 10));

            if (resizeableComponent && altPressed) {
                if (direction > 0) {
                    resizeableComponent.NextSize(abs);
                }
                else {
                    resizeableComponent.PrevSize(abs);
                }
            }
            else if (rotatingComponent) {
                var freeRotatingComponent = GetComponent<RotatableFree>();

                // Изменение оси вращения по нажатию ctrl
                if (freeRotatingComponent && ctrlDown) {
                    freeRotatingComponent.NextRotationAxis();
                    shouldUpdateRotation = true;
                }

                // Изменение вращения по колесику мыши

                if (direction > 0) {
                    rotatingComponent.IncreaseRotationIndex(abs);
                }
                else {
                    rotatingComponent.DecreaseRotationIndex(abs);
                }
                shouldUpdateRotation = true;
            }
        }
        else if(controller != null) {
            if(controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip)) {
                rotatingComponent.IncreaseRotationIndex(1);
            }
        }

        if (rotatingComponent && shouldUpdateRotation) {
            transform.rotation = PropertyManager.MainBuildStation.transform.rotation * rotatingComponent.GetRotation();
        }
    }

    public override void StartInteract(Transform instigator = null) {
        base.StartInteract(instigator);
        var rotatingComponent = GetComponent<Rotatable>();
        var resizeableComponent = GetComponent<Resizable>();

        if (resizeableComponent) {
            resizeableComponent.SaveSizeIndex();
        }

        // Загружаем поворот этого объекта из менеджера, если он не был поднят ранее
        if (!wasPickedUpBefore) {
            if (rotatingComponent) {
                rotatingComponent.LoadRotationIndex();
            }
            wasPickedUpBefore = true;
            if (spawner) {
                spawner.SpawningAllowed = false;
            }
        }
        else {
            // либо сохраняем вращение в менеджер, если объект уже поднимался
            if (rotatingComponent) {
                rotatingComponent.SaveRotationIndex();
            }
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
