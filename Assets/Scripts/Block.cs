using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

// Отдельный блок сетки редактора
public class Block {

    // Внутренние переменные
    private GameObject obj = null;
    private GameObject anchor;
    private bool isFilled = false;
    private MeshRenderer debugRenderer;

    // Конструктор
    public Block(Vector3 position, Vector3i coord, Transform parent, GameObject anchor) {
        affectingBlock = null;
        this.anchor = anchor;
        debugRenderer = anchor.GetComponentInChildren<MeshRenderer>();
        if (debugRenderer) {
            debugRenderer.enabled = false;
        }
        anchor.transform.position = position;
        anchor.transform.parent = parent;
        this.coord = coord;
        updateAnchorName();
    }


    // Интерфейс

    // Координаты блока
    public readonly Vector3i coord;

    // Блок, хранящий в себе GameObject, который занимает и этот блок
    public Block affectingBlock {
        get;
        private set;
    }

    // Список блоков, занятых текущим GameObject'ом
    public List<Block> affectedBlocks {
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
    public void fill(GameObject obj, bool collide, Vector3 offset, Quaternion rotation, List<Block> affectedBlocks = null) {

        if (this.obj) {
            empty();
        }

        this.obj = obj;

        if (affectedBlocks == null) {
            affectedBlocks = new List<Block>();
        }

        var objTransform = getObjectTransform();
        if (objTransform) {
            objTransform.parent = anchor.transform;
        }

        show(collide);
        setPosition(offset, rotation);

        isFilled = true;

        this.affectedBlocks = affectedBlocks;

        foreach (Block affectedBlock in affectedBlocks) {
            affectedBlock.fill(this);
        }

        updateAnchorName();

        if (debugRenderer) {
            debugRenderer.enabled = true;
        }
    }

    // Убирает GameObject или ссылку на блок с ним из блока
    public void empty() {
        var objTransform = getObjectTransform();
        if (objTransform && objTransform.parent == anchor.transform) {
            objTransform.localPosition = Vector3.zero;
            objTransform.localRotation = Quaternion.identity;
            objTransform.parent = null;
        }
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

    // Опусташает блоки, задетые текущим GameObject'ом
    private void emptyAffected() {
        if (affectedBlocks == null) return;

        foreach (Block affectedBlock in affectedBlocks) {
            if(affectedBlock == this) {
                Debug.LogError("Block is overlapping itself");
                continue;
            }
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

    // Устанавливает позицию блока
    public void setPosition(Vector3 offset, Quaternion rotation) {
        if (!obj) return;

        var objTransform = getObjectTransform();
        if (objTransform && objTransform.parent == anchor.transform) {
            objTransform.localRotation = rotation;
            objTransform.localPosition = offset;
        }
    }

    public void updateAnchorName() {
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
    public Transform getObjectTransform() {
        if (!obj) return null;
        var blockTransform = obj.transform.parent;

        if (!blockTransform || !blockTransform.gameObject.GetComponent<Throwable>()) {
            blockTransform = obj.transform;
        }
        return blockTransform;
    }


}
