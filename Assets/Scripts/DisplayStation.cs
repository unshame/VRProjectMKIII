using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Редактор здания, который отображает контент по указаниям другого редактора
// Создает копии передаваемых объектов для отображения и удаляет их, когда они покидают редактор
// Можно включить/отключить кисть
public class DisplayStation : BuildStation {

    public bool shouldShowBrush = false;
    [HideInInspector]
    public Vector3 scaleDif;

    public void CalculateScaleDif(Vector3 parentBlockSize) {
        scaleDif = new Vector3(
            blockSize.x / parentBlockSize.x,
            blockSize.y / parentBlockSize.y,
            blockSize.z / parentBlockSize.z
        );
    }

    public override void RemoveObject(GameObject obj) {
        base.RemoveObject(obj);
        Destroy(obj);
    }

    public void RemoveBlock(Coord blockCoord) {
        var block = GetBlock(blockCoord);
        if (block != null && block.gameObjectOrigin != null) {
            var gameObject = block.gameObjectOrigin;
            block.empty();
            Destroy(gameObject);
        }
    }

    public override void AddObject(Coord blockCoord, GameObject obj, List<Block> affectedBlocks) {
        var blockCopy = Instantiate(obj);
        blockCopy.transform.localScale = Vector3.Scale(blockCopy.transform.localScale, scaleDif);
        base.AddObject(blockCoord, blockCopy, affectedBlocks);
    }

    public override void ShowBrush(Coord blockCoord, GameObject obj) {
        if (shouldShowBrush) {
            base.ShowBrush(blockCoord, obj);
        }
    }

    protected override void OnTriggerStay(Collider other) {
    }

    protected override void OnTriggerExit(Collider other) {
    }
}