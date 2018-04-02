using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Редактор здания, который отображает контент по указаниям другого редактора
// Создает копии передаваемых объектов для отображения и удаляет их, когда они покидают редактор
// Можно включить/отключить кисть
public class DisplayStation : BuildStation {

    public bool ShouldShowBrush = false;

    public override void RemoveBlock(GameObject otherBlock) {
        base.RemoveBlock(otherBlock);
        Destroy(otherBlock);
    }

    public void RemoveBlock(Coord blockCoord) {
        var block = GetBlock(blockCoord);
        if (block != null && block.gameObjectOrigin != null) {
            var gameObject = block.gameObjectOrigin;
            block.empty();
            Destroy(gameObject);
        }
    }

    public override void AddBlock(Coord blockCoord, GameObject otherBlock, Vector3 offset, List<Block> affectedBlocks) {
        var blockCopy = Instantiate(otherBlock);
        var otherCollider = otherBlock.GetComponent<BoxCollider>();
        offset = new Vector3(otherCollider.size.x, otherCollider.size.y, otherCollider.size.z) * blockSize / 2;
        base.AddBlock(blockCoord, blockCopy, offset, affectedBlocks);
    }

    public override void ShowBrush(Coord blockCoord, Mesh mesh, Vector3 offset) {
        if (ShouldShowBrush) {
            base.ShowBrush(blockCoord, mesh, offset);
        }
    }

    protected override void OnTriggerStay(Collider other) {
    }

    protected override void OnTriggerExit(Collider other) {
    }
}