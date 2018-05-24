using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BumScript : MonoBehaviour {


    public struct MindBlock {

        public MindBlock(GameObject b, int a) {
            block = b;
            blockSize = a;
        }
        public GameObject block;
        public int blockSize;
    }
    public List<MindBlock> blocks = new List<MindBlock>();
    private int summaryBlocksAffected = 0;
    private int wallBlocks = 0;
    private int roofBlocks = 0;
    private int doorBlocks = 0;
    private int windowBlocks = 0;

    public GameObject bubble;
    public Material wallMat;
    public Material roofMat;
    public Material doorMat;
    public Material windowMat;

    public RuntimeAnimatorController goodAnim;
    public RuntimeAnimatorController badAnim;
    public RuntimeAnimatorController defaultAnim;


    private void Start() {
        bubble.GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Animator>().runtimeAnimatorController = defaultAnim;
    }

    public void updateDecision() {
        var renderer = bubble.GetComponent<MeshRenderer>();
        var animator = GetComponent<Animator>();
        renderer.enabled = true;
        animator.runtimeAnimatorController = badAnim;

        if (wallBlocks <= 200) {
            renderer.material = wallMat;
            return;
        }

        if (roofBlocks <= 100) {
            renderer.material = roofMat;
            return;
        }

        if (doorBlocks == 0) {
            renderer.material = doorMat;
            return;
        }

        if (windowBlocks < 4) {
            renderer.material = windowMat;
            return;
        }

        bubble.GetComponent<MeshRenderer>().enabled = false;
        animator.runtimeAnimatorController = goodAnim;
    }
    public void BlockAdded(GameObject obj, Vector3i objBlockMagnitude) {
        ObjectIdentity identity = obj.GetComponent<ObjectIdentity>();
        int afflectedBlocks = objBlockMagnitude.x * objBlockMagnitude.y * objBlockMagnitude.z;
        summaryBlocksAffected += afflectedBlocks;

        if (identity.groupName == "wall") {
            wallBlocks += afflectedBlocks;
        }
        else if (identity.groupName == "roof") {
            roofBlocks += afflectedBlocks;
        }
        else if (identity.groupName == "door") {
            doorBlocks++;
        }
        else if (identity.groupName == "window") {
            windowBlocks++;
        }

        MindBlock mb = new MindBlock(obj, afflectedBlocks);
        blocks.Add(mb);
        updateDecision();

    }
    public void BlockDeleted(GameObject obj) {
        ObjectIdentity identity = obj.GetComponent<ObjectIdentity>();
        int deletedBlocks = 0;

        for(int i = 0; i < blocks.Count; i++) {
            var mb = blocks[i];
            if (mb.block == obj) {
                summaryBlocksAffected -= mb.blockSize;
                deletedBlocks = mb.blockSize;
            }
        }

        if (identity.groupName == "wall") {
            wallBlocks -= deletedBlocks;
        }
        if (identity.groupName == "roof") {
            roofBlocks -= deletedBlocks;
        }
        if (identity.groupName == "door") {
            doorBlocks--;
        }
        if (identity.groupName == "window") {
            windowBlocks--;
        }

        //updateDecision();
        ResetAnim();


    }

    public void Reset() {
        summaryBlocksAffected = 0;
        wallBlocks = 0;
        roofBlocks = 0;
        doorBlocks = 0;
        windowBlocks = 0;
        ResetAnim();
    }

    private void ResetAnim() {
        var renderer = bubble.GetComponent<MeshRenderer>();
        var animator = GetComponent<Animator>();
        renderer.enabled = false;
        animator.runtimeAnimatorController = defaultAnim;
    }
}
