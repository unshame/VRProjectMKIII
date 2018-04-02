using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayStation : BuildStation {

    public override void RemoveBlock(GameObject otherBlock) {
        base.RemoveBlock(otherBlock);
        Destroy(otherBlock);
    }

    public void RemoveBlock(Coord blockCoord) {
        var block = GetBlock(blockCoord);
        if (block != null && block.gameObject != null) {
            var gameObject = block.gameObject;
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
    }

    public override void HideBrush() {
    }

    protected override void OnTriggerStay(Collider other) {
    }

    protected override void OnTriggerExit(Collider other) {
    }
}