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
        displayStation.SetParentStation(this);
    }

    public override void HideBrush() {
        base.HideBrush();
        displayStation.HideBrush();
    }

    public override void ShowBrush(Coord blockCoord, GameObject obj, Quaternion rotation) {
        base.ShowBrush(blockCoord, obj, rotation);
        displayStation.ShowBrush(blockCoord, obj, rotation);
    }

    public override void RemoveObject(GameObject obj) {
        var objCoord = GetObjectCoord(obj);
        base.RemoveObject(obj);
        displayStation.RemoveObject(objCoord);
    }

    public override void AddObject(Coord blockCoord, GameObject obj, List<Block> affectedBlocks, Quaternion rotation) {
        displayStation.AddObject(blockCoord, obj, affectedBlocks, rotation);
        base.AddObject(blockCoord, obj, affectedBlocks, rotation);
    }
}
