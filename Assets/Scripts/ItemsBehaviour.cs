using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsBehaviour : MonoBehaviour
{
	public GameObject Item;
    // Use this for initialization
    void Start()
    {
		GameObject StaticPrefab;

		StaticPrefab = Instantiate (Item, new Vector3(this.transform.position.x,this.transform.position.y+2f,this.transform.position.z), this.transform.rotation);
		StaticPrefab.transform.parent = this.transform;	
		StaticPrefab.transform.localScale = new Vector3 (0.1f, 0.1f, 0.1f);
		StaticPrefab.transform.eulerAngles = new Vector3 (0f, 0f, 0f);

	
	}


    // Update is called once per frame
    void Update()
    {
		//this.transform.localScale += 0.1;
    }
    
}