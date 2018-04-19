using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Методы этого класса нужно подключить к контроллеру, чтобы легко узнавать, производится ли действие с объектом
public class Interactable : MonoBehaviour {

    public bool isActive = false;

    [SerializeField]
    private bool _isLocked = false;

    public bool isLocked {
        get { return _isLocked; }
        set {
            gameObject.layer = value ? 0 : 8;
            _isLocked = value;
        }
    }

    public virtual void StartInteract(Transform instigator = null) {
        if (isLocked) {
            throw new System.Exception("Attempting to interact with a locked object");
        }
        isActive = true;
    }

    public virtual void StopInteract() {
        isActive = false;
    }
}
