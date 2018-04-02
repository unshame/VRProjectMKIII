using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Редактор здания, который отображает свой контент в привязанном редакторе
public class DualBuildStation : BuildStation {


    public DisplayStation displayStation;

    protected override void Start() {
        if (size != displayStation.size) {
            Debug.LogWarning("DualBuildStation: Size will be set equal to DisplayStation's size");
            size = displayStation.size;
        }
        base.Start();
    }

    public override void HideBrush() {
        base.HideBrush();
        displayStation.HideBrush();
    }

    public override void ShowBrush(Coord blockCoord, Mesh mesh, Vector3 offset) {
        base.ShowBrush(blockCoord, mesh, offset);
        displayStation.ShowBrush(blockCoord, mesh, offset);
    }

    public override void RemoveBlock(GameObject otherBlock) {
        var otherBlockCoord = GetBlockCoord(otherBlock);
        base.RemoveBlock(otherBlock);
        displayStation.RemoveBlock(otherBlockCoord);
    }

    public override void AddBlock(Coord blockCoord, GameObject otherBlock, Vector3 offset, List<Block> affectedBlocks) {
        base.AddBlock(blockCoord, otherBlock, offset, affectedBlocks);
        displayStation.AddBlock(blockCoord, otherBlock, offset, affectedBlocks);
    }
}
