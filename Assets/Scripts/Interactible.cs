using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

// Методы этого класса нужно подключить к контроллеру, чтобы легко узнавать, производится ли действие с объектом
public class Interactible : MonoBehaviour {

    public bool isActive = false;

    [SerializeField]
    private bool _isLocked = false;

    public bool isLocked {
        get { return _isLocked; }
        set {
            var collider = GetComponent<Collider>();
            if (collider) {
                collider.enabled = !value;
            }
            _isLocked = value;
        }
    }

    public virtual void StartInteract(Transform instigator) {
        if (isLocked) {
            throw new System.Exception("Attempting to interact with a locked object");
        }
        isActive = true;
    }

    public virtual void StopInteract() {
        isActive = false;
    }
}
