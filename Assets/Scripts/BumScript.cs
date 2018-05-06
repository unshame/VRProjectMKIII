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
		foreach (Block[][] block in buildStation.blocks) {
			foreach (Block[] blo in block) {
				foreach (Block b in blo) {
					if (b.isFilled) {
						try{
							ObjectIdentity obj = b.affectingBlock.gameObjectOrigin.GetComponent<ObjectIdentity> ();
							if(obj.typeName == "cube2"){
								Debug.Log("Hello");
							}
						}
						catch{
							
						}
					}
				}
			}
		}
	}
}
