using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumScript : MonoBehaviour {

	private BuildStation buildStation;
	private int summaryBlocksAffected;
	private int wallBlocks;
	private int roofBlocks;
	private int doorBlocks;
	private int windowBlocks;
	public  void setParentStation(BuildStation bs) {
		// Устанавливаем главенствующий редактор
		buildStation = bs;
		summaryBlocksAffected = 0;
		wallBlocks= 0;
		roofBlocks= 0;
		doorBlocks= 0;
		windowBlocks= 0;
	}
	public void updateDecision(){
        /*var size = buildStation.size;
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                for (int z = 0; z < size.z; z++) {
                    var block = buildStation.blocks[x][y][z];
                    if (block.isFilled) {
                        var identity = block.gameObject.GetComponent<ObjectIdentity>();
                        if(identity.typeName == "wall") {
                            Debug.Log("+");
                        }
                    }
				}
			}
		}*/
		Debug.Log ("Walls: " + wallBlocks);
		Debug.Log ("Roofs: " + roofBlocks);
		Debug.Log ("Doors: " + doorBlocks);
		Debug.Log ("Windows: " + windowBlocks);
		if (wallBlocks >= 200 && roofBlocks >= 100 && doorBlocks != 0 && windowBlocks != 0)
			Debug.Log ("Well played!");
	}
	public void BlockAdded(GameObject obj){
		ObjectIdentity identity = obj.GetComponent<ObjectIdentity> ();
		summaryBlocksAffected += identity.blocksAffected;
		if (identity.typeName == "wall")
			wallBlocks += identity.blocksAffected;
		if (identity.typeName == "roof")
			roofBlocks += identity.blocksAffected;
		if (identity.typeName == "door")
			doorBlocks++;
		if (identity.typeName == "window")
			windowBlocks++;
		updateDecision ();	

	}
	public void BlockDeleted(GameObject obj){
		ObjectIdentity identity = obj.GetComponent<ObjectIdentity> ();
		summaryBlocksAffected -= identity.blocksAffected;
		if (identity.typeName == "wall")
			wallBlocks -= identity.blocksAffected;
		if (identity.typeName == "roof")
			roofBlocks -= identity.blocksAffected;
		if (identity.typeName == "door")
			doorBlocks--;
		if (identity.typeName == "window")
			windowBlocks--;
		updateDecision ();	

	}
}
