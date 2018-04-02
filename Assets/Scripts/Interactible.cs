using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Для запоминания, находится ли объект в руке игрока
public class Interactible : MonoBehaviour {

    public bool isActive = false;

    public virtual void StartInteract(Transform instigator) {
        isActive = true;
    }

    public virtual void StopInteract() {
        isActive = false;
    }
}
