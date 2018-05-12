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
		//Debug.Log ("Walls: " + wallBlocks);
		//Debug.Log ("Roofs: " + roofBlocks);
		//Debug.Log ("Doors: " + doorBlocks);
		//Debug.Log ("Windows: " + windowBlocks);
		if (wallBlocks <= 200)
			Debug.Log ("Not Enough walls");
		else if (roofBlocks <= 100)
			Debug.Log ("Not Enough roofs");
		else if (doorBlocks == 0)
			Debug.Log ("Put the door");
		else if (windowBlocks == 0)
			Debug.Log ("Need some windows");
		else
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
	public void Reset(){
		summaryBlocksAffected = 0;
		wallBlocks= 0;
		roofBlocks= 0;
		doorBlocks= 0;
		windowBlocks= 0;
	}
}
