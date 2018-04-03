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
    private BuildStation parent;
    private GameObject _savedObj;

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

    public void RemoveObject(Coord blockCoord) {
        var block = GetBlock(blockCoord);
        if (block != null && block.gameObjectOrigin != null) {
            var obj = block.gameObjectOrigin;
            block.empty();
            Destroy(obj);
        }
    }

    public override void AddObject(Coord blockCoord, GameObject obj, List<Block> affectedBlocks, Quaternion rotation) {
        var blockCopy = Instantiate(obj);
        blockCopy.transform.localScale = Vector3.Scale(blockCopy.transform.localScale, scaleDif);
        var affectedBlocksCopy = new List<Block>();
        foreach(Block affectedBlock in affectedBlocks) {
            affectedBlocksCopy.Add(GetBlock(affectedBlock.coord));
        }
        _savedObj = obj;
        base.AddObject(blockCoord, blockCopy, affectedBlocksCopy, rotation * obj.transform.localRotation);
        _savedObj = null;
    }

    protected override Quaternion CalculateRotation(GameObject obj) {
        return base.CalculateRotation(_savedObj);
    }

    public override void ShowBrush(Coord blockCoord, GameObject obj, Quaternion rotation) {
        if (shouldShowBrush) {
            base.ShowBrush(blockCoord, obj, rotation);
        }
    }

    protected override void OnTriggerStay(Collider other) {
    }

    protected override void OnTriggerExit(Collider other) {
    }
}