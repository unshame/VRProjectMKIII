using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

// Отдельный блок сетки редактора
public class Block {

    // Внутренние переменные
    private GameObject obj = null;                   // Объект добавленный в блок
    private GameObject anchor;                       // Объект, представляющий блок в игровом мире
    private Transform holder;                        // Объект внутри anchor, держащий добавленный блок
    private bool isFilled = false;                   // Заполнен ли блок
    private MeshRenderer debugRenderer;              // Визуальное отображение блока    
    public Vector3i spaces;                          // Свободное место после блока
    public Vector3i connectedAfter = -Vector3i.one;
    public Vector3i connectedBefore = -Vector3i.one;
    public Vector3i objBlockReach = -Vector3i.one;

    // Конструктор
    public Block(Vector3 position, Vector3i coord, Transform parent, GameObject anchor, Vector3i spaces) {
        affectingBlock = null;

        // Устанавливаем позицию объекта, представляющего блок в игровом мире
        this.anchor = anchor;
        anchor.transform.parent = parent;
        anchor.transform.position = position;

        // Сохраняем и выключаем визуальное отображение блока
        debugRenderer = anchor.GetComponentInChildren<MeshRenderer>();
        if (debugRenderer) {
            debugRenderer.enabled = false;
        }

        // Запоминаем т выключаем держатель блока
        holder = anchor.transform.GetChild(anchor.transform.childCount - 1);
        holder.GetComponent<BlockHolder>().enabled = false;

        this.coord = coord;
        this.spaces = spaces;

        updateAnchorName();
    }


    // Координаты блока
    public readonly Vector3i coord;

    // Блок, хранящий в себе GameObject, который занимает и этот блок
    public Block affectingBlock {
        get;
        private set;
    }

    // Пуст ли блок
    public bool isEmpty {
        get {
            return !obj && !isFilled;
        }
    }

    // Позиция блока без сдвига
    public Vector3 position {
        get {
            return anchor.transform.position;
        }
    }

    // GameObject, хранящийся в этом блоке или в другом блоке, если он накладывается на этот блок
    public GameObject gameObject {
        get {
            return affectingBlock == null ? obj : affectingBlock.gameObjectOrigin;
        }
    }

    // GameObject, хранящийся исключительно в этом блоке
    public GameObject gameObjectOrigin {
        get {
            return obj;
        }
    }


    // GameObject, хранящийся исключительно в этом блоке, совпадает с переданным
    public bool has(GameObject obj) {
        return this.obj != null && this.obj == obj;
    }


    // Заполняет блок без GameObject'a
    public void fill() {

        if (obj) {
            empty();
        }

        isFilled = true;

        updateAnchorName();

        if (debugRenderer) {
            debugRenderer.enabled = true;
        }
    }

    // Заполняет блок, сохраняя ссылку на блок с GameObject'ом
    public void fill(Block affectingBlock) {
        this.affectingBlock = affectingBlock;
        fill();
    }

    // Заполняет блок переданным GameObject'ом
    public void fill(GameObject obj, bool collide, Vector3 offset, Quaternion rotation, Vector3i objBlockReach) {

        if (this.obj) {
            empty();
        }

        this.obj = obj;
        this.objBlockReach = objBlockReach;

        // Помещаем объект в держатель объекта и включаем его
        addObjectToHolder(offset, rotation);

        // Показываем блок и устанавливаем его позицию и поворот
        show(collide);

        isFilled = true;

        updateAnchorName();

        if (debugRenderer) {
            debugRenderer.enabled = true;
        }
    }

    public void fill(GameObject obj, bool collide, Vector3 offset, Quaternion rotation) {
        fill(obj, collide, offset, rotation, -Vector3i.one);
    }
    // Убирает GameObject или ссылку на блок с ним из блока
    public void empty() {

        // Убираем объект из держателя и отключаем держатель
        removeObjectFromHolder();

        obj = null;
        objBlockReach = -Vector3i.one;
        isFilled = false;
        
        affectingBlock = null;
        
        updateAnchorName();

        if (debugRenderer) {
            debugRenderer.enabled = false;
        }
    }

    // Убирает GameObject только если он совпадает с переданным
    public void empty(GameObject obj) {

        if (this.obj == obj) {
            empty();
        }
    }

    // Опустошает блок, выкидывая объект в случайную сторону
    public void eject(float force = 5) {
        if (!obj) return;
        var rigidbody = obj.transform.GetComponent<Rigidbody>();
        empty();

        if (rigidbody) {
            rigidbody.isKinematic = false;
            var x = Random.Range(0.3f, 1);
            var z = Random.Range(0.3f, 1);
            if(x > 0.65f) {
                z = -z;
            }
            if (z > 0.65f) {
                x = -x;
            }
            var velocity = new Vector3(x, 1, z);
            rigidbody.velocity = velocity * force;
            rigidbody.angularVelocity = velocity / 2;
        }
    }


    // Прячет GameObject в этом блоке
    public void hide() {
        setBlockStatus(true, false, false);
    }

    // Показывает GameObject в этом блоке, опционально включая коллизию
    public void show(bool collide = false) {
        setBlockStatus(true, true, collide);
    }

    // Включает/выключает Rigidbody, Renderer и Collider GameObject'a в блоке
    public void setBlockStatus(bool kinematic, bool visible, bool collide) {
        if (!obj) return;

        var rigidbody = obj.transform.GetComponent<Rigidbody>();

        if (rigidbody) {
            rigidbody.isKinematic = kinematic;
        }

        obj.GetComponent<Renderer>().enabled = visible;
        obj.GetComponent<Collider>().enabled = collide;
    }


    // Добавляет блок в держатель, включает держатель, устанавливает его позицию и поворот
    private void addObjectToHolder(Vector3 offset, Quaternion rotation) {

        obj.transform.parent = holder;

        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        holder.localPosition = offset;
        holder.localRotation = rotation;

        holder.GetComponent<BlockHolder>().enabled = true;
    }

    // Убирает блок из держателя, отключает блок
    private void removeObjectFromHolder() {
        if (obj && obj.transform.parent == holder) {

            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            obj.transform.parent = null;

            holder.GetComponent<BlockHolder>().enabled = false;
        }
    }

    // Обновляет имя объекта, представляющего блок в игровом мире
    private void updateAnchorName() {
        var name = coord.ToString();
        if (!isEmpty) {
            name += " (";
            if (obj) {
                name += "Base '" + obj.name + "'";
            }
            else {
                name += "'" + affectingBlock.obj.name + "'";
            }
            name += ")";
        }
        anchor.name = name;
    }
}
