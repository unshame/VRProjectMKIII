using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumScript : MonoBehaviour {

	private BuildStation buildStation;
	public  void setParentStation(BuildStation bs) {
		// Устанавливаем главенствующий редактор
		buildStation = bs;
	}
	public void updateDecision(){
        var size = buildStation.size;
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                for (int z = 0; z < size.z; z++) {
                    var block = buildStation.blocks[x][y][z];
                    if (block.isFilled) {
                        var identity = block.gameObject.GetComponent<ObjectIdentity>();
                        if(identity.typeName == "cube") {
                            Debug.Log("+");
                        }
                    }
				}
			}
		}
	}
}
