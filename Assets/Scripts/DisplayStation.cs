using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Редактор здания, который отображает контент по указаниям другого редактора
// Создает копии передаваемых объектов для отображения и удаляет их, когда они покидают редактор
// Можно включить/отключить кисть
public class DisplayStation : BuildStation {

    [HideInInspector]
    public Vector3 scaleDif;
    private BuildStation parent;

    public void SetParentStation(BuildStation parentStation) {
        parent = parentStation;
        scaleDif = new Vector3(
            blockSize.x / parent.blockSize.x,
            blockSize.y / parent.blockSize.y,
            blockSize.z / parent.blockSize.z
        );
    }

    public override void RemoveObject(GameObject obj) {
        base.RemoveObject(obj);
        Destroy(obj);
    }

    public void RemoveObject(Vector3i blockCoord) {
        var block = GetBlock(blockCoord);
        if (block != null && block.gameObjectOrigin != null) {
            var obj = block.gameObjectOrigin;
            block.empty();
            Destroy(obj);
        }
    }

    public override void AddObject(Vector3i blockCoord, GameObject obj, Block[] affectedBlocks, Quaternion rotation) {
        var objCopy = Instantiate(obj);
        objCopy.transform.localScale = Vector3.Scale(objCopy.transform.localScale, scaleDif);
        Block[] affectedBlocksCopy = null;
        if (affectedBlocks != null) {
            affectedBlocksCopy = new Block[affectedBlocks.Length];
            for (var i = 0; i < affectedBlocks.Length; i++) {
                if (affectedBlocks[i] != null) {
                    affectedBlocksCopy[i] = GetBlock(affectedBlocks[i].coord);
                }
            }
        }
        var identityCopy = objCopy.GetComponent<ObjectIdentity>();
        var identity = obj.GetComponent<ObjectIdentity>();
        if (identityCopy) {
            identityCopy.rotationAxis = identity.rotationAxis;
            identityCopy.SetRotationIndex(identity.GetRotationIndex());
        }
        base.AddObject(blockCoord, objCopy, affectedBlocksCopy, rotation * obj.transform.localRotation);
    }

    public override void ShowBrush(Vector3i blockCoord, GameObject obj, Quaternion rotation) {
        // TODO: может быть сделать, чтобы он корректно работал
    }

    protected override void OnTriggerStay(Collider other) {
    }

    protected override void OnTriggerExit(Collider other) {
    }
}