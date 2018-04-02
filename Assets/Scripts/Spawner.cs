using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(BoxCollider))]
public class Spawner : MonoBehaviour {
    public GameObject Prefab;
    public float SpawnDelay;
    public bool IsEmpty = true;

    private float TimePast = 0f;

    List<GameObject> ObjectsInside = new List<GameObject>();

    bool IsCorrectType(GameObject other) {
        var identity = Prefab.gameObject.GetComponentInChildren<ObjectIdentity>();
        var otherIdentity = other.GetComponent<ObjectIdentity>();
        return identity && otherIdentity && identity.TypeName == otherIdentity.TypeName;
    }

    public void Start() {
        GetComponent<BoxCollider>().isTrigger = true;
        SpawnObject(Prefab);
    }

    void Update() {

        if(ObjectsInside.Count != 0) {
            TimePast = 0;
            IsEmpty = false;
            return;
        }

        TimePast += Time.deltaTime;
        IsEmpty = true;

        if (TimePast > SpawnDelay) {
            SpawnObject(Prefab);
            TimePast = 0;
            IsEmpty = false;
        }
    }

    public void SpawnObject(GameObject Item) {
        Instantiate(Item, transform.position, transform.rotation);
    }

    void OnTriggerStay(Collider other) {
        GameObject otherObject = other.gameObject;
        if (IsCorrectType(otherObject)) {
            if (!ObjectsInside.Contains(otherObject)) {
                ObjectsInside.Add(otherObject);
            }
        }
    }

    void OnTriggerExit(Collider other) {
        GameObject otherObject = other.gameObject;
        if (IsCorrectType(otherObject)) {
            if (ObjectsInside.Contains(otherObject)) {
                ObjectsInside.Remove(otherObject);
            }
        }
    }
}
