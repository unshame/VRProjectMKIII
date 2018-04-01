using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkplaceRotator : MonoBehaviour {


    // Use this for initialization
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        transform.transform.rotation *= transform.localRotation;
        transform.localRotation = Quaternion.identity;
    }
}
