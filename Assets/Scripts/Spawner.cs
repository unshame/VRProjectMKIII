using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
	public Transform Spawnpoint; // Координаты спавна
	public GameObject Prefab; // Модель
	public TriggerSpawn TriggerArea; // область триггера спавна
	public enum SType {RigidSpawner,StaticSpawner,RigidWithoutTrigger};
	public SType SpawnerType;
	public void Start(){
        SpawnObject(Spawnpoint, Prefab);
        if (SpawnerType.ToString() != "RigidWithoutTrigger") {
			SpawnTriggerArea (Spawnpoint, TriggerArea);
		}
	}
    public void SpawnObject(Transform Point, GameObject Item) {
        if (SpawnerType.ToString() == "StaticSpawner") {
            SpawnStaticObject(Spawnpoint, Prefab);
        }
        else {
            SpawnRigidObject(Spawnpoint, Prefab);
        }
    }
	// Функция спавна объекта
	// *Point - точка респавна
	// *Item - Объект
	public void SpawnRigidObject(Transform Point, GameObject Item){
		GameObject RigidPrefab;
		RigidPrefab = Instantiate (Item, Point.position, Point.rotation);
		RigidPrefab.gameObject.AddComponent <Rigidbody>();
		RigidPrefab.name = Prefab.name;
		RigidPrefab.gameObject.tag = "SpItem";
	}
	public void SpawnStaticObject(Transform Point, GameObject Item){
		GameObject StaticPrefab;
		StaticPrefab = Instantiate (Item, Point.position, Point.rotation);
		StaticPrefab.name = Prefab.name;
		StaticPrefab.gameObject.tag = "SpItem";
	}
    // Спавн Триггер области
	public void SpawnTriggerArea(Transform Point, TriggerSpawn Area){
		TriggerSpawn One;
		One = Instantiate (Area, Point.position, Point.rotation) as TriggerSpawn;
		One.IncludeSpawner (this);
		One.SetName (Area.name + " " + Prefab.name);
		One.gameObject.tag = "TriggerArea";
	}
}
