using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

// Отдельный блок сетки редактора
public class Block {

    // Внутренние переменные
    private GameObject obj = null;         // Объект добавленный в блок
    private GameObject anchor;             // Объект, представляющий блок в игровом мире
    private Transform holder;              // Объект внутри anchor, держащий добавленный блок
    private bool isFilled = false;         // Заполнен ли блок
    private MeshRenderer debugRenderer;    // Визуальное отображение блока

    // Конструктор
    public Block(Vector3 position, Vector3i coord, Transform parent, GameObject anchor) {
        affectingBlock = null;

        // Устанавливаем позицию объекта, представляющего блок в игровом мире
        this.anchor = anchor;
        anchor.transform.position = position;
        anchor.transform.parent = parent;

        // Сохраняем и выключаем визуальное отображение блока
        debugRenderer = anchor.GetComponentInChildren<MeshRenderer>();
        if (debugRenderer) {
            debugRenderer.enabled = false;
        }

        // Запоминаем т выключаем держатель блока
        holder = anchor.transform.GetChild(anchor.transform.childCount - 1);
        holder.GetComponent<BlockHolder>().enabled = false;

        this.coord = coord;

        updateAnchorName();
    }


    // Координаты блока
    public readonly Vector3i coord;

    // Блок, хранящий в себе GameObject, который занимает и этот блок
    public Block affectingBlock {
        get;
        private set;
    }

    // Список блоков, занятых текущим GameObject'ом
    public Block[] affectedBlocks {
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
    public void fill(GameObject obj, bool collide, Vector3 offset, Quaternion rotation, Block[] affectedBlocks = null) {

        if (this.obj) {
            empty();
        }

        this.obj = obj;

        // Помещаем объект в держатель объекта и включаем его
        addObjectToHolder(offset, rotation);

        // Показываем блок и устанавливаем его позицию и поворот
        show(collide);

        isFilled = true;

        // Запоминаем и заполняем блоки, задетые объектом
        if (affectedBlocks != null) {

            this.affectedBlocks = affectedBlocks;

            for (var i = 0; i < affectedBlocks.Length; i++) {
                if (affectedBlocks[i] != null) {
                    affectedBlocks[i].fill(this);
                }
            }
        }

        updateAnchorName();

        if (debugRenderer) {
            debugRenderer.enabled = true;
        }
    }

    // Убирает GameObject или ссылку на блок с ним из блока
    public void empty() {

        // Убираем объект из держателя и отключаем держатель
        removeObjectFromHolder();

        obj = null;
        isFilled = false;
        
        affectingBlock = null;
        emptyAffected();
        
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

    // Опустошает блоки, задетые текущим GameObject'ом
    private void emptyAffected() {
        if (affectedBlocks == null) return;

        for (var i = 0; i < affectedBlocks.Length; i++) { 
            var affectedBlock = affectedBlocks[i];
            if (affectedBlock == this) {
                Debug.LogError("Block is overlapping itself");
                continue;
            }
            if(affectedBlock == null) continue;
            affectedBlock.empty();
        }

        affectedBlocks = null;
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

        var rigidbody = obj.transform.parent ? obj.transform.parent.GetComponent<Rigidbody>() : obj.GetComponent<Rigidbody>();

        if (!rigidbody) {
            rigidbody = obj.GetComponent<Rigidbody>();
        }

        if (rigidbody) {
            rigidbody.isKinematic = kinematic;
        }

        obj.GetComponent<Renderer>().enabled = visible;
        obj.GetComponent<Collider>().enabled = collide;
    }


    // Добавляет блок в держатель, включает держатель, устанавливает его позицию и поворот
    private void addObjectToHolder(Vector3 offset, Quaternion rotation) {

        var objTransform = getObjectTransform();
        if (objTransform) {
            objTransform.parent = holder;

            objTransform.localPosition = Vector3.zero;
            objTransform.localRotation = Quaternion.identity;

            holder.localPosition = offset;
            holder.localRotation = rotation;

            holder.GetComponent<BlockHolder>().enabled = true;
        }

    }

    // Убирает блок из держателя, отключает блок
    private void removeObjectFromHolder() {
        var objTransform = getObjectTransform();
        if (objTransform && objTransform.parent == holder) {

            objTransform.localPosition = Vector3.zero;
            objTransform.localRotation = Quaternion.identity;

            objTransform.parent = null;

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

    // Возвращает transform объекта или его parent'a
    private Transform getObjectTransform() {
        if (!obj) return null;
        var blockTransform = obj.transform.parent;

        if (!blockTransform || !blockTransform.gameObject.GetComponent<Throwable>()) {
            blockTransform = obj.transform;
        }
        return blockTransform;
    }
}
