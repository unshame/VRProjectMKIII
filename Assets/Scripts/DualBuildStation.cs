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
        displayStation.CalculateScaleDif(blockSize);
    }

    public override void HideBrush() {
        base.HideBrush();
        displayStation.HideBrush();
    }

    public override void ShowBrush(Coord blockCoord, GameObject obj) {
        base.ShowBrush(blockCoord, obj);
        displayStation.ShowBrush(blockCoord, obj);
    }

    public override void RemoveObject(GameObject obj) {
        var objCoord = GetObjectCoord(obj);
        base.RemoveObject(obj);
        displayStation.RemoveBlock(objCoord);
    }

    public override void AddObject(Coord blockCoord, GameObject obj, List<Block> affectedBlocks) {
        base.AddObject(blockCoord, obj, affectedBlocks);
        displayStation.AddObject(blockCoord, obj, affectedBlocks);
    }
}
