using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkplaceRotator : Interactible {

    private Transform parent;
    private Transform instigator;
    private Vector3 startOtherAngle;
    private Vector3 startAngle;

    // Use this for initialization
    void Start() {
        parent = transform.parent.transform;
    }

    // Update is called once per frame
    void Update() {
        if(isActive) {
            parent.eulerAngles = startAngle + (startOtherAngle - instigator.eulerAngles)*4;
        }
    }

    public override void StartInteract(Transform instigator) {
        startOtherAngle = instigator.eulerAngles;
        startAngle = parent.eulerAngles;
        this.instigator = instigator;
        isActive = true;
        
    }

    public override void StopInteract() {
        isActive = false;
    }
}
