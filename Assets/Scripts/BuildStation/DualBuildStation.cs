using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Редактор здания, который отображает свой контент в привязанном редакторе
public class DualBuildStation : BuildStation {


    public DisplayStation displayStation;
	public BumScript BumMind;

    protected override void Awake() {
        base.Awake();

        // Устанавливаем главенствующий редактор
        displayStation.SetParentStation(this);
		BumMind.setParentStation (this);
    }


    public override void HideBrush() {
        base.HideBrush();
        displayStation.HideBrush();
    }

    public override void ShowBrush(Vector3i blockCoord, GameObject obj, Quaternion rotation) {
        base.ShowBrush(blockCoord, obj, rotation);
        displayStation.ShowBrush(blockCoord, obj, rotation);
    }

    public override void RemoveObject(GameObject obj) {
        var objCoord = GetObjectCoord(obj);

        base.RemoveObject(obj);

        // Удаляем блок в дисплее по координатам
        displayStation.RemoveObject(objCoord);
		BumMind.BlockDeleted (obj);
		BumMind.updateDecision ();
    }

    public override void AddObject(Vector3i blockCoord, GameObject obj, Quaternion rotation, Vector3i objBlockMagnitude) {
        displayStation.AddObject(blockCoord, obj, rotation, objBlockMagnitude);
        base.AddObject(blockCoord, obj, rotation, objBlockMagnitude);
		BumMind.BlockAdded (obj,objBlockMagnitude);
    }

    public override void Clear() {
        if (editable) {
            displayStation.Clear();
        }
        base.Clear();
		BumMind.Reset ();
		BumMind.updateDecision ();
    }
}
