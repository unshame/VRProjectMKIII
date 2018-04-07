using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Редактор здания, который отображает контент по указаниям другого редактора
// Создает копии передаваемых объектов для отображения и удаляет их, когда они покидают редактор
// Можно включить/отключить кисть
public class DisplayStation : BuildStation {

    private Vector3 scaleDif;       // Различие между масштабами редактора и дисплея
    private BuildStation parent;    // Редактор, главенствующий над этим редактором
    private bool started = false;   // Для того, чтобы запустить этот редактор перед главенствующим

    protected override void Start() {

        if (!started) {
            started = true;
            base.Start();
        }
    }

    // Устанавливает главенствующий редактор, считает различие масштабов
    public void SetParentStation(BuildStation parentStation) {
        parent = parentStation;

        // Запускаем редактор, если он еще не запущен, чтобы знать размер блоков
        if (!started) {
            started = true;
            base.Start();
        }

        // Считаем различие между масштабами редактора и дисплея
        scaleDif = VectorUtils.Divide(blockSize, parent.blockSize);
    }

    public override void RemoveObject(GameObject obj) {
        base.RemoveObject(obj);

        // Удаляем объект
        Destroy(obj);
    }

    // Удаляет объект по координатам блока
    public void RemoveObject(Vector3i blockCoord) {
        var block = GetBlock(blockCoord);

        // Проверяем, что блок существует и не пуст
        if (block != null && block.gameObjectOrigin != null) {
            var obj = block.gameObjectOrigin;
            block.empty();

            // Удаляем объект
            Destroy(obj);
        }
    }

    public override void AddObject(Vector3i blockCoord, GameObject obj, Block[] affectedBlocks, Quaternion rotation) {

        // Копируем объект
        var objCopy = Instantiate(obj);

        // Корректируем масштаб объекта
        objCopy.transform.localScale = Vector3.Scale(objCopy.transform.localScale, scaleDif);

        // Копируем задетые блоки
        Block[] affectedBlocksCopy = null;
        if (affectedBlocks != null) {
            affectedBlocksCopy = new Block[affectedBlocks.Length];
            for (var i = 0; i < affectedBlocks.Length; i++) {
                if (affectedBlocks[i] != null) {
                    affectedBlocksCopy[i] = GetBlock(affectedBlocks[i].coord);
                }
            }
        }

        // Копируем identity блока
        var identityCopy = objCopy.GetComponent<ObjectIdentity>();

        if (identityCopy) {
            identityCopy.CopyIdentity(obj);
        }

        // Добавляем блок в сетку с поворотом редактора
        base.AddObject(blockCoord, objCopy, affectedBlocksCopy, rotation * obj.transform.localRotation);
    }

    public override void ShowBrush(Vector3i blockCoord, GameObject obj, Quaternion rotation) {
        // TODO: может быть сделать, чтобы он корректно работал
    }

    public override void Clear() {
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                for (int z = 0; z < size.z; z++) {
                    var obj = blocks[x][y][z].gameObject;
                    if (obj) {
                        blocks[x][y][z].eject();
                        Destroy(obj);
                    }
                }
            }
        }
        objList.Clear();
    }

    // Не реагируем на коллизии
    protected override void OnTriggerStay(Collider other) {
    }

    protected override void OnTriggerExit(Collider other) {
    }
}