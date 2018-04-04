using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockHolder : MonoBehaviour {

	void Update () {
        if (transform.childCount == 0) return;
        var obj = transform.GetChild(0);
        if (obj) {
            obj.localPosition = Vector3.zero;
            obj.localRotation = Quaternion.identity;
        }
	}
}
