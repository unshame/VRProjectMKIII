using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretChest : MonoBehaviour {

    public GameObject treasure;

    void OnCollisionEnter(Collision collision) {
        foreach (ContactPoint contact in collision.contacts) {
            if(contact.otherCollider.gameObject.tag == "key") {
                Instantiate(treasure, transform.position, transform.rotation);
                Destroy(gameObject);
                return;
            }
        }
    }
}
