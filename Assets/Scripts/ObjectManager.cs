using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour {
	public GameObject[] Items;
	public GameObject[] Spawners;
	void Start () {
		
	}

	void Update () {
		Items = GameObject.FindGameObjectsWithTag ("ChildSpItem");
        foreach(GameObject Item in Items)
        {
			if(false && Vector3.Distance(Item.transform.position, Camera.main.transform.position) > 100f)
			{
			    Item.tag ="ReadyForDestroy";
                Destroy(Item, 4f);
			}
        }
    }
}
