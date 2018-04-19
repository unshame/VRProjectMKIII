using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Спавнит объекты, когда они покидают коллайдер спавнера
[RequireComponent(typeof(BoxCollider))]
public class Spawner : MonoBehaviour {

    // Префаб, который будет инстанцироваться
    private GameObject Prefab;

    // Время до спавна нового объекта
    public float SpawnDelay;

    // Пуст ли коллайдер
    [HideInInspector]
    public bool IsEmpty = true;

    public bool SpawningAllowed = true;

    // Время с последнего момента, когда коллайдер не был пустым
    private float TimePast = 0f;

    List<GameObject> ObjectsInside = new List<GameObject>();

    // Объект имеет TypeName в ObjectIdentity, совпадающий с префабом
    bool IsCorrectType(GameObject other) {
        var identity = Prefab.gameObject.GetComponentInChildren<ObjectIdentity>();
        var otherIdentity = other.GetComponent<ObjectIdentity>();
        return identity && otherIdentity && identity.typeName == otherIdentity.typeName;
    }

    // Устанавливает коллайдер в качестве триггера и спавнит первый объект
    public void Start() {
        Prefab = transform.GetChild(0).gameObject;
        Prefab.SetActive(false);
        GetComponent<BoxCollider>().isTrigger = true;
        SpawnObject(Prefab);
    }

    // Спавнит объект, если такого нет внутри коллайдера и пришло время
    void Update() {

        // Коллайдер не пуст
        if (ObjectsInside.Count != 0) {
            TimePast = 0;
            IsEmpty = false;
            return;
        }

        // Коллайдер пуст
        TimePast += Time.deltaTime;
        IsEmpty = true;

        // Пришло время спавнить
        if (SpawningAllowed && TimePast > SpawnDelay) {
            SpawnObject(Prefab);
            TimePast = 0;
            IsEmpty = false;
        }
    }

    // Спавнит объект
    public void SpawnObject(GameObject Item) {
        var item = Instantiate(Item, transform.position, transform.rotation);
        item.SetActive(true);
        var interactible = item.GetComponentInChildren<InteractableBlock>();
        if (interactible) {
            interactible.spawner = this;
        }
        var rotatingComponent = item.GetComponentInChildren<Rotatable>();
        if (rotatingComponent) {
            rotatingComponent.InitRotations();
            item.transform.rotation = RotationManager.MainBuildStation.transform.rotation * rotatingComponent.GetRotation();
        }
    }

    // Добавляет объект в список объектов внутри коллайдера, если он подходящий
    void OnTriggerStay(Collider other) {
        GameObject otherObject = other.gameObject;
        if (IsCorrectType(otherObject)) {
            if (!ObjectsInside.Contains(otherObject)) {
                ObjectsInside.Add(otherObject);
            }
        }
    }

    // Удаляет объект из списка объектов внутри коллайдера
    void OnTriggerExit(Collider other) {
        GameObject otherObject = other.gameObject;
        if (ObjectsInside.Contains(otherObject)) {
            ObjectsInside.Remove(otherObject);
        }
    }
}
