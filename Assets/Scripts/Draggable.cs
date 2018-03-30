using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : Interactible {

    public float maxDistance = 5;

    private Transform oldParent;
    private Rigidbody rigidbody;

    // Use this for initialization
    void Start() {
        oldParent = transform.parent;
        rigidbody = transform.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        if(isActive) {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }
    }

    public override void StartInteract(Transform instigator) {
        if (Vector3.Distance(instigator.position, transform.position) > maxDistance) return;
        GetComponent<Collider>().enabled = true;
        isActive = true;
        transform.parent = Camera.main.transform;
        rigidbody.useGravity = false;
        rigidbody.isKinematic = false;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }

    public override void StopInteract() {
        transform.parent = oldParent;
        rigidbody.useGravity = true;
        isActive = false;
    }
}