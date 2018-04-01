using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSpawn : MonoBehaviour {
	private Spawner Factory;
	private bool IsEmpty = false;
    private float TimePast = 0f;
    public float SpawnDelay;

    void OnTriggerStay(Collider other)
    {
		if (Factory.Prefab.transform.GetChild(0).name == other.gameObject.name && other.tag != "ReadyForDestroy")
        {
            IsEmpty = false;
            TimePast = 0;
        }
    }

    void OnTriggerExit(Collider other){
		GameObject ExitedObject = other.gameObject;
		if (Factory.Prefab.transform.GetChild(0).name == ExitedObject.name && other.tag != "ReadyForDestroy") {
            if (other.gameObject.tag == "SpItem") other.gameObject.tag = "ChildSpItem";
			IsEmpty = true;
            TimePast = 0;
		}
	}

	void Update(){
        if (!IsEmpty) return;
        TimePast += Time.deltaTime;
        if(TimePast > SpawnDelay) {
            Factory.SpawnObject(Factory.Spawnpoint, Factory.Prefab);
            TimePast = 0;
        }
    }
	// Функция подключеня к спавнеру
	public void IncludeSpawner(Spawner Sp){
		this.Factory = Sp;
		SetScale (Sp.Prefab);
	}
	// Функция изменения размера
	public void SetScale(GameObject Obj){
		float a = 1f;
		transform.localScale = new Vector3 (Obj.transform.localScale.x *a, Obj.transform.localScale.y * a, Obj.transform.localScale.z *a);
	}
	public void SetName(string Name){
		this.name = Name;
	}

}
