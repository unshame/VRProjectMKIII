using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Block {
    private GameObject block = null;
    private GameObject anchor;
    private bool isFilled = false;
    private List<Block> affectedBlocks;
    private Vector3 offset;

    public Block affectingBlock {
        get;
        private set;
    }

    public bool isEmpty {
        get {
            return !block && !isFilled;
        }
    }

    public Vector3 position {
        get {
            return anchor.transform.position;
        }
    }

    public GameObject gameObjectOrigin {
        get {
            return block;
        }
    }

    public GameObject gameObject {
        get {
            return affectingBlock == null ? block : affectingBlock.gameObjectOrigin;
        }
    }

    public bool sameAs(GameObject block) {
        return this.block != null && this.block == block;
    }

    public Block(Vector3 position, Quaternion rotation, Transform parent) {
        affectingBlock = null;
        anchor = new GameObject();
        anchor.transform.position = position;
        anchor.transform.rotation = rotation;
        anchor.transform.parent = parent;
    }

    public void fill() {
        if (this.block) {
            empty();
        }
        isFilled = true;
    }

    public void fill(Block affectingBlock) {
        fill();
        this.affectingBlock = affectingBlock;
    }

    public void fill(GameObject block, bool collide, Vector3 offset, Quaternion rotation, List<Block> affectedBlocks = null) {

        if (this.block) {
            empty();
        }

        this.block = block;

        if(affectedBlocks == null) {
            affectedBlocks = new List<Block>();
        }

        this.offset = offset;
        show(collide);
        resetPosition(rotation);

        isFilled = true;

        this.affectedBlocks = affectedBlocks;

        foreach (Block affectedBlock in affectedBlocks) {
            affectedBlock.fill(this);
        }
    }

    public void empty() {
        block = null;
        isFilled = false;
        affectingBlock = null;
        emptyAffected();
    }

    public void empty(GameObject block) {

        if (this.block == block) {
            empty();
        }
    }

    private void emptyAffected() {
        if (affectedBlocks == null) return;

        foreach (Block affectedBlock in affectedBlocks) {
            affectedBlock.empty();
        }

        affectedBlocks = null;
    }

    public void hide() {
        if (!block) return;

        var rigidbody = block.transform.parent ? block.transform.parent.GetComponent<Rigidbody>() : block.GetComponent<Rigidbody>();
        if (!rigidbody) {
            rigidbody = block.GetComponent<Rigidbody>();
        }
        if (rigidbody) {
            rigidbody.isKinematic = true;
        }
        block.GetComponent<Renderer>().enabled = false;
        block.GetComponent<Collider>().enabled = false;
    }

    public void show(bool collide = false) {
        if (!block) return;

        var rigidbody = block.transform.parent ? block.transform.parent.GetComponent<Rigidbody>() : block.GetComponent<Rigidbody>();
        if (!rigidbody) {
            rigidbody = block.GetComponent<Rigidbody>();
        }
        if (rigidbody) {
            rigidbody.isKinematic = true;
        }
        block.GetComponent<Renderer>().enabled = true;
        block.GetComponent<Collider>().enabled = collide;
    }

    public void resetPosition(Quaternion rotation) {
        if (!block) return;
        var blockTransform = block.transform.parent;
        if (!blockTransform || !blockTransform.gameObject.GetComponent<Throwable>()) {
            blockTransform = block.transform;
        }
        if (blockTransform) {
            blockTransform.position = anchor.transform.position + rotation * offset;
            blockTransform.rotation = anchor.transform.rotation;
        }
    }
}

public class Coord {
    public int x, y, z;
    public Coord(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

public class BuildStation : MonoBehaviour {

    public int size = 10;
    protected float blockSize;

    public GameObject brush;
    public Material brushMaterial;
    protected Block brushBlock = null;

    public List<List<List<Block>>> blocks = new List<List<List<Block>>>();
    public List<GameObject> blocksList = new List<GameObject>();

    public bool editable = true;

    void Start() {
        blockSize = transform.localScale.x / size;

        brush = Instantiate(brush, transform.parent);
        brush.GetComponent<Renderer>().material = brushMaterial;
        brush.GetComponent<Rigidbody>().isKinematic = true;
        brush.GetComponent<Renderer>().enabled = false;
        brush.GetComponent<Collider>().enabled = false;
        brush.transform.localScale = new Vector3(blockSize, blockSize, blockSize);

        var offset = size * blockSize / 2;
        var position = transform.position - new Vector3(offset, offset, offset);

        for (int x = 0; x < size; x++) {
            var dimension = new List<List<Block>>();

            for (int y = 0; y < size; y++) {
                var axis = new List<Block>();

                for (int z = 0; z < size; z++) {

                    var block = new Block(
                        position + new Vector3(x, y, z) * blockSize, 
                        Quaternion.identity, 
                        transform.parent
                    );

                    axis.Add(block);
                }

                dimension.Add(axis);
            }

            blocks.Add(dimension);
        }
    }

    void Update() {
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                for (int z = 0; z < size; z++) {
                    blocks[x][y][z].resetPosition(transform.rotation);
                }
            }
        }
    }

    public virtual void HideBrush() {

        if (brushBlock != null) {
            brushBlock.hide();
            brushBlock.empty();
            brushBlock = null;
        }
    }

    public virtual void ShowBrush(Coord blockCoord, Mesh mesh, Vector3 offset) {
        var block = GetBlock(blockCoord);
        block.fill(brush, false, offset, transform.rotation);
        brushBlock = block;
        brush.GetComponent<MeshFilter>().mesh = mesh;
        brush.transform.localScale = new Vector3(blockSize, blockSize, blockSize);
    }

    public Block GetBlock(int x, int y, int z) {
        return GetBlock(new Coord(x, y, z));
    }

    public Block GetBlock(Coord coord) {

        if(coord.x >= size || coord.y >= size || coord.z >= size) {
            return null;
        }

        return blocks[coord.x][coord.y][coord.z];
    }

    public Coord GetBlockCoord(Block block) {
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                for (int z = 0; z < size; z++) {
                    if (block == blocks[x][y][z]) {
                        return new Coord(x, y, z);
                    };
                    
                }
            }
        }
        return null;
    }

    public Coord GetBlockCoord(GameObject block) {
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                for (int z = 0; z < size; z++) {
                    if (blocks[x][y][z].sameAs(block)) {
                        return new Coord(x, y, z);
                    };

                }
            }
        }
        return null;
    }

    public virtual void RemoveBlock(GameObject otherBlock) {

        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                for (int z = 0; z < size; z++) {
                    var block = blocks[x][y][z];
                    block.empty(otherBlock);
                }
            }
        }

        blocksList.Remove(otherBlock);

        var otherTransform = otherBlock.transform.parent;
        if (otherTransform == null) {
            otherTransform = otherBlock.transform;
        }
        if (otherTransform.parent == transform.parent) {
            otherTransform.parent = null;
        }
    }

    public virtual void AddBlock(Coord blockCoord, GameObject otherBlock, Vector3 offset, List<Block> affectedBlocks) {
        var block = GetBlock(blockCoord);
        otherBlock.transform.localScale = new Vector3(blockSize, blockSize, blockSize);
        var otherTransform = otherBlock.transform.parent;
        if(otherTransform == null) {
            otherTransform = otherBlock.transform;
        }
        otherTransform.parent = transform.parent;
        blocksList.Add(otherBlock);
        block.fill(otherBlock, true, offset, transform.rotation, affectedBlocks);
    }

    protected virtual void OnTriggerStay(Collider other) {
        if (!editable) {
            HideBrush();
            return;
        }
        var otherBlock = other.gameObject;
        Vector3 offset;
        List<Block> closestAffectedBlocks;
        bool otherIsActive;
        var closestBlockCoord = GetClosestBlockCoord(otherBlock, out offset, out closestAffectedBlocks, out otherIsActive);

        if (closestBlockCoord != null) {
            if (otherIsActive) {
                ShowBrush(closestBlockCoord, otherBlock.GetComponent<MeshFilter>().mesh, offset);
            }
            else {
                AddBlock(closestBlockCoord, otherBlock, offset, closestAffectedBlocks);
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other) {
        HideBrush();
    }

    protected Coord GetClosestBlockCoord(GameObject otherBlock, out Vector3 offset, out List<Block> closestAffectedBlocks, out bool isActive) {
        offset = new Vector3();
        closestAffectedBlocks = null;
        isActive = false;

        var interactible = otherBlock.GetComponent<Interactible>();

        if (!interactible) {
            return null;
        }

        isActive = interactible.isActive;
        if (blocksList.Contains(otherBlock)) {
            if (isActive) {
                RemoveBlock(otherBlock);
            }
            else {
                return null;
            }
        }

        HideBrush();

        var minDist = Mathf.Infinity;
        Coord closestBlockCoord = null;


        var otherCollider = otherBlock.GetComponent<BoxCollider>();
        offset = new Vector3(otherCollider.size.x, otherCollider.size.y, otherCollider.size.z) * blockSize / 2;
        var otherPosition = otherBlock.transform.position - transform.rotation * offset;

        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                for (int z = 0; z < size; z++) {
                    var blockCoord = new Coord(x, y, z);
                    List<Block> affectedBlocks;

                    var block = GetValidBlock(blockCoord, otherBlock.GetComponent<BoxCollider>().size, out affectedBlocks);
                    if (block == null) {
                        continue;
                    }

                    var dist = Vector3.Distance(otherPosition, block.position);
                    if (dist < blockSize * 4 && dist < minDist) {
                        closestBlockCoord = new Coord(x, y, z);
                        minDist = dist;
                        closestAffectedBlocks = affectedBlocks;
                    }
                }
            }
        }
        return closestBlockCoord;
    }

    Block GetValidBlock(Coord blockCoord, Vector3 otherBlockSize, out List<Block> affectedBlocks) {
        var block = GetBlock(blockCoord);
        var isValid = false;

        if (blockCoord.y == 0) {
            isValid = true;
        }
        else {
            var connectedBlocks = GetConnectedBlocks(blockCoord.x, blockCoord.y, blockCoord.z);
            foreach (Block connectedBlock in connectedBlocks) {
                if (!connectedBlock.isEmpty && !connectedBlock.sameAs(brush)) {
                    isValid = true;
                }
            }
        }

        affectedBlocks = new List<Block>();

        if (isValid) {
            for (int i = 1; i <= 3; i++) {
                isValid = AppendAffectedBlocks(affectedBlocks, blockCoord, otherBlockSize, i);
                if (!isValid) break;
            }
        }

        isValid = isValid && (block.isEmpty || block.sameAs(brush));
        
        return isValid ? block : null;
    }

    bool AppendAffectedBlocks(List<Block> affectedBlocks, Coord blockCoord, Vector3 otherBlockSize, int dimension) {

        var sizeCoord = new Coord(
            blockCoord.x + Mathf.CeilToInt(otherBlockSize.x) - 1, 
            blockCoord.y + Mathf.CeilToInt(otherBlockSize.y) - 1, 
            blockCoord.z + Mathf.CeilToInt(otherBlockSize.z) - 1
        );

        Block affectedBlock = null;
        var i = dimension == 1 ? sizeCoord.x : (dimension == 2 ? sizeCoord.y : sizeCoord.z);
        var cmpi = dimension == 1 ? blockCoord.x : (dimension == 2 ? blockCoord.y : blockCoord.z);

        while (i > cmpi) {

            if(dimension == 1) {
                affectedBlock = GetBlock(i, sizeCoord.y, sizeCoord.z);
            }
            else if (dimension == 2) {
                affectedBlock = GetBlock(sizeCoord.x, i, sizeCoord.z);
            }
            else {
                affectedBlock = GetBlock(sizeCoord.x, sizeCoord.y, i);
            }

            if (affectedBlock != null) {
                if (affectedBlock.isEmpty) {
                    affectedBlocks.Add(affectedBlock);
                }
                else {
                    return false;
                }
            }

            i--;
        }
        return true;
    }

    List<Block> GetConnectedBlocks(int x, int y, int z) {
        List<Block> connectedBlocks = new List<Block>();

        if (x > 0) {
            connectedBlocks.Add(blocks[x - 1][y][z]);
        }

        if (x < size - 1) {
            connectedBlocks.Add(blocks[x + 1][y][z]);
        }

        if (y > 0) {
            connectedBlocks.Add(blocks[x][y - 1][z]);
        }

        if (y < size - 1) {
            connectedBlocks.Add(blocks[x][y + 1][z]);
        }

        if (z > 0) {
            connectedBlocks.Add(blocks[x][y][z - 1]);
        }

        if (z < size - 1) {
            connectedBlocks.Add(blocks[x][y][z + 1]);
        }

        return connectedBlocks;
    }

}
