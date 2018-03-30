using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactible : MonoBehaviour {

    public bool isActive = false;

    public virtual void StartInteract(Transform instigator) {
        
    }

    public virtual void StopInteract() {

    }
}
