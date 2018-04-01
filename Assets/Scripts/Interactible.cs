using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactible : MonoBehaviour {

    public bool isActive = false;

    public virtual void StartInteract(Transform instigator) {
        isActive = true;
    }

    public virtual void StopInteract() {
        isActive = false;
    }
}
