using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {

    public Vector3 rotationPerUpdate = new Vector3(0, 0, 0);

	void Update () {
        transform.Rotate(rotationPerUpdate * Time.deltaTime, Space.World);
	}
}
