using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

// Отдельный блок сетки редактора
public class Block {

    // Внутренние переменные
    private GameObject block = null;
    private GameObject anchor;
    private bool isFilled = false;
    private Vector3 offset;


    // Конструктор
    public Block(Vector3 position, Quaternion rotation, Transform parent) {
        affectingBlock = null;
        anchor = new GameObject();
        anchor.transform.position = position;
        anchor.transform.rotation = rotation;
        anchor.transform.parent = parent;
    }


    // Интерфейс

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
            return !block && !isFilled;
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
            return affectingBlock == null ? block : affectingBlock.gameObjectOrigin;
        }
    }

    // GameObject, хранящийся исключительно в этом блоке
    public GameObject gameObjectOrigin {
        get {
            return block;
        }
    }


    // GameObject, хранящийся исключительно в этом блоке, совпадает с переданным
    public bool sameAs(GameObject block) {
        return this.block != null && this.block == block;
    }


    // Заполняет блок без GameObject'a
    public void fill() {
        if (this.block) {
            empty();
        }
        isFilled = true;
    }

    // Заполняет блок, сохраняя ссылку на блок с GameObject'ом
    public void fill(Block affectingBlock) {
        fill();
        this.affectingBlock = affectingBlock;
    }

    // Заполняет блок переданным GameObject'ом
    public void fill(GameObject block, bool collide, Vector3 offset, Quaternion rotation, List<Block> affectedBlocks = null) {

        if (this.block) {
            empty();
        }

        this.block = block;

        if (affectedBlocks == null) {
            affectedBlocks = new List<Block>();
        }

        this.offset = offset;
        show(collide);
        setPosition(rotation);

        isFilled = true;

        this.affectedBlocks = affectedBlocks;

        foreach (Block affectedBlock in affectedBlocks) {
            affectedBlock.fill(this);
        }
    }

    // Убирает GameObject или ссылку на блок с ним из блока
    public void empty() {
        block = null;
        isFilled = false;
        affectingBlock = null;
        emptyAffected();
    }

    // Убирает GameObject только если он совпадает с переданным
    public void empty(GameObject block) {

        if (this.block == block) {
            empty();
        }
    }

    // Опусташает блоки, задетые текущим GameObject'ом
    private void emptyAffected() {
        if (affectedBlocks == null) return;

        foreach (Block affectedBlock in affectedBlocks) {
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
        if (!block) return;

        var rigidbody = block.transform.parent ? block.transform.parent.GetComponent<Rigidbody>() : block.GetComponent<Rigidbody>();

        if (!rigidbody) {
            rigidbody = block.GetComponent<Rigidbody>();
        }

        if (rigidbody) {
            rigidbody.isKinematic = kinematic;
        }

        block.GetComponent<Renderer>().enabled = visible;
        block.GetComponent<Collider>().enabled = collide;
    }

    // Устанавливает позицию блока
    public void setPosition(Quaternion rotation) {
        if (!block) return;

        var blockTransform = block.transform.parent;

        if (!blockTransform || !blockTransform.gameObject.GetComponent<Throwable>()) {
            blockTransform = block.transform;
        }

        if (blockTransform) {
            blockTransform.position = anchor.transform.position + rotation * offset;
            blockTransform.rotation = anchor.transform.rotation;
        }
    }
}
