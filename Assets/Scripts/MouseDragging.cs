using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDragging : MonoBehaviour {

    public Transform empty;
    public LayerMask interactLayer;
    private float dist = 0;

    private Transform target;

    // Update is called once per frame
    void Update() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit h;
        if (Physics.Raycast(transform.position, ray.direction, out h, interactLayer) && target == null) {
            if (Input.GetMouseButton(0)) {
                dist = Vector3.Distance(h.point, transform.position);
                empty.position = transform.position + (ray.direction.normalized * dist);

                target = h.transform;
            }
        }

        Debug.Log(transform.position);

        Rigidbody rb = null;

        if (target != null) rb = target.GetComponent<Rigidbody>();

        if (Input.GetMouseButton(0) && rb) {
            rb.isKinematic = true;
            target.transform.parent = empty;
            empty.position = transform.position + (ray.direction.normalized * dist);
        }

        if (!Input.GetMouseButton(0) && rb) {
            rb.isKinematic = false;
            target.transform.parent = null;
            target = null;
        }
    }
}
