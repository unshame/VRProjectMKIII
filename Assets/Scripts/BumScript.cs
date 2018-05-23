using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumScript : MonoBehaviour {

	private BuildStation buildStation;
	public struct MindBlock
	{
		
		public MindBlock(GameObject b,int a){
			block = b;
			blockSize =a;
		}
		public GameObject block;
		public int blockSize;
	}
	public List<MindBlock> blocks = new List<MindBlock>();
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
	public void BlockAdded(GameObject obj,Vector3i objBlockMagnitude){
		ObjectIdentity identity = obj.GetComponent<ObjectIdentity> ();
		int afflectedBlocks = objBlockMagnitude.x * objBlockMagnitude.y * objBlockMagnitude.z;
		summaryBlocksAffected += afflectedBlocks;
		if (identity.typeName == "wall")
			wallBlocks += afflectedBlocks;
		if (identity.typeName == "roof")
			roofBlocks += afflectedBlocks;
		if (identity.typeName == "door")
			doorBlocks++;
		if (identity.typeName == "window")
			windowBlocks++;
		MindBlock mb = new MindBlock (obj, afflectedBlocks);
		blocks.Add(mb);
		updateDecision ();	

	}
	public void BlockDeleted(GameObject obj){
		ObjectIdentity identity = obj.GetComponent<ObjectIdentity> ();
		int deletedBlocks = 0;
		foreach (MindBlock mb in blocks) 
		{
			if (mb.block == obj) {
				summaryBlocksAffected -= mb.blockSize;
				deletedBlocks = mb.blockSize;
			}
		}
		if (identity.typeName == "wall")
			wallBlocks -= deletedBlocks;
		if (identity.typeName == "roof")
			roofBlocks -= deletedBlocks;
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
