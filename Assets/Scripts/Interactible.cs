using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Методы этого класса нужно подключить к контроллеру, чтобы легко узнавать, производится ли действие с объектом
public class Interactible : MonoBehaviour {

    public bool isActive = false;

    public virtual void StartInteract(Transform instigator) {
        isActive = true;
    }

    public virtual void StopInteract() {
        isActive = false;
    }
}
