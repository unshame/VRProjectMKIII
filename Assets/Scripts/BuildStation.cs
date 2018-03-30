using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block {
    private GameObject block = null;
    private GameObject anchor;
    private bool isFilled = false;
    private List<Block> affectedBlocks;

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

    public bool sameAs(GameObject block) {
        return this.block != null && this.block == block;
    }

    public Block(Vector3 position, Quaternion rotation, Transform parent) {
        anchor = new GameObject();
        anchor.transform.position = position;
        anchor.transform.rotation = rotation;
        anchor.transform.parent = parent;
    }

    public void fill() {
        isFilled = true;
    }

    public void fill(GameObject block, bool collide, Vector3 offset, List<Block> affectedBlocks = null) {

        if (this.block) {
            hide();
        }

        this.block = block;

        if(affectedBlocks == null) {
            affectedBlocks = new List<Block>();
        }

        block.transform.position = anchor.transform.position + offset;
        block.transform.rotation = anchor.transform.rotation;
        show(collide);

        isFilled = true;

        this.affectedBlocks = affectedBlocks;
        
        foreach (Block affectedBlock in affectedBlocks) {
            affectedBlock.fill();
        }
    }

    public void empty() {
        block = null;
        isFilled = false;
        emptyAffected();
    }

    public void empty(GameObject block) {

        if (this.block == block) {
            this.block = null;
            isFilled = false;
            emptyAffected();
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

        block.GetComponent<Rigidbody>().isKinematic = true;
        block.GetComponent<Renderer>().enabled = false;
        block.GetComponent<Collider>().enabled = false;
    }

    public void show(bool collide = false) {
        if (!block) return;

        block.GetComponent<Rigidbody>().isKinematic = true;
        block.GetComponent<Renderer>().enabled = true;
        block.GetComponent<Collider>().enabled = collide;
    }
}

public struct Coord {
    public int x, y, z;
    public Coord(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

public class BuildStation : MonoBehaviour {

    public int size = 10;
    public float blockSize = 1;

    public GameObject brush;
    public Material brushMaterial;
    private Block brushBlock = null;

    private List<List<List<Block>>> blocks = new List<List<List<Block>>>();
    private List<GameObject> blocksList = new List<GameObject>();

    // Use this for initialization
    void Start() {
        brush = Instantiate(brush, transform.parent);
        brush.GetComponent<Renderer>().material = brushMaterial;
        brush.GetComponent<Rigidbody>().isKinematic = true;
        brush.GetComponent<Renderer>().enabled = false;
        brush.GetComponent<Collider>().enabled = false;

        var offset = size * blockSize / 2;
        var position = transform.position - new Vector3(offset, offset, offset);

        for (int x = 0; x < size; x++) {
            var dimension = new List<List<Block>>();

            for (int y = 0; y < size; y++) {
                var axis = new List<Block>();

                for (int z = 0; z < size; z++) {

                    var block = new Block(
                        new Vector3(
                            position.x + x * blockSize, 
                            position.y + y * blockSize, 
                            position.z + z * blockSize), 
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

    void HideBrush() {

        if (brushBlock != null) {
            brushBlock.hide();
            brushBlock.empty();
            brushBlock = null;
        }
    }

    void ShowBrush(Block block, Mesh mesh, Vector3 offset) {
        block.fill(brush, false, offset);
        brushBlock = block;
        brush.GetComponent<MeshFilter>().mesh = mesh;
    }

    Block GetBlock(int x, int y, int z) {
        return GetBlock(new Coord(x, y, z));
    }

    Block GetBlock(Coord coord) {

        if(coord.x >= size || coord.y >= size || coord.z >= size) {
            return null;
        }

        return blocks[coord.x][coord.y][coord.z];
    }

    void RemoveBlock(GameObject otherBlock) {

        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                for (int z = 0; z < size; z++) {
                    var block = blocks[x][y][z];
                    block.empty(otherBlock);
                }
            }
        }

        blocksList.Remove(otherBlock);

        if (otherBlock.transform.parent == transform.parent) {
            otherBlock.transform.parent = null;
        }
    }

    void AddBlock(Coord blockCoord, GameObject otherBlock, Vector3 offset, List<Block> affectedBlocks) {
        var block = GetBlock(blockCoord);
        otherBlock.transform.parent = transform.parent;
        blocksList.Add(otherBlock);
        block.fill(otherBlock, true, offset, affectedBlocks);
    }

    void OnTriggerStay(Collider other) {
        
        var interactible = other.GetComponent<Interactible>();
        var otherBlock = other.gameObject;

        if (!interactible) {
            return;
        }

        if (blocksList.Contains(otherBlock)) {
            if (interactible.isActive) {
                RemoveBlock(otherBlock);
            }
            else {
                return;
            }
        }

        HideBrush();

        var minDist = Mathf.Infinity;
        Coord minBlockCoord = new Coord();
        List<Block> minAffectedBlocks = null;
        Block closestBlock = null;

        var otherCollider = other.GetComponent<BoxCollider>();
        var offset = transform.rotation * (new Vector3(otherCollider.size.x, otherCollider.size.y, otherCollider.size.z) * blockSize / 2);
        var otherPosition = other.transform.position - offset;

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
                    if (dist < blockSize*4 && dist < minDist) {
                        closestBlock = block;
                        minDist = dist;
                        minBlockCoord = blockCoord;
                        minAffectedBlocks = affectedBlocks;
                    }
                }
            }
        }

        if (closestBlock != null) {
            if (interactible.isActive) {
                ShowBrush(closestBlock, otherBlock.GetComponent<MeshFilter>().mesh, offset);
            }
            else {
                AddBlock(minBlockCoord, otherBlock, offset, minAffectedBlocks);
            }
        }
    }

    void OnTriggerExit(Collider other) {
        HideBrush();
    }

    Block GetValidBlock(Coord blockCoord, Vector3 size, out List<Block> affectedBlocks) {
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

        var sizeCoord = new Coord(
            blockCoord.x + Mathf.CeilToInt(size.x) - 1, 
            blockCoord.y + Mathf.CeilToInt(size.y) - 1, 
            blockCoord.z + Mathf.CeilToInt(size.z) - 1
        );

        affectedBlocks = new List<Block>();

        if (isValid) {
            for (int i = 1; i <= 3; i++) {
                isValid = AppendAffectedBlocks(affectedBlocks, blockCoord, sizeCoord, i);
                if (!isValid) break;
            }
        }

        isValid = isValid && (block.isEmpty || block.sameAs(brush));
        
        return isValid ? block : null;
    }

    bool AppendAffectedBlocks(List<Block> affectedBlocks, Coord blockCoord, Coord sizeCoord, int dimension) {
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
