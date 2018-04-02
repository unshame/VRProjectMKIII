using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Координаты блока в 3D массиве
public class Coord {
    public int x, y, z;
    public Coord(int x, int y, int z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

// Редактор здания
public class BuildStation : MonoBehaviour {

    // Кол-во клеток по одной стороне
    public int size = 10;
    
    // Размер блока (рассчитывается из size)
    protected float blockSize;

    // Префаб, а после инициализации - инстанция кисти-блока (полупрозрачный блок)
    public GameObject brush;

    // Материал кисти
    public Material brushMaterial;

    // Блок, в котором находится кисть
    protected Block brushBlock = null;

    // 3D массив блоков
    [HideInInspector]
    public List<List<List<Block>>> blocks = new List<List<List<Block>>>();

    // Массив всех GameObject'ов в редакторе
    [HideInInspector]
    public List<GameObject> blocksList = new List<GameObject>();

    // Можно ли изменять контент редактора в игре
    public bool editable = true;

    // Максимальная удаленность валидного блока от объекта, который планируется туда поставить (дистанция в блоках)
    public int maxBlockDistance = 4;

    // Инициализирует кисть, создает блоки
    protected virtual void Start() {
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

    // Восстанавливает позиции всех GameObject, т.к. SteamVR их решает двигать
    protected virtual void Update() {
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                for (int z = 0; z < size; z++) {
                    blocks[x][y][z].setPosition(transform.rotation);
                }
            }
        }
    }

    // Прячет кисть
    public virtual void HideBrush() {

        if (brushBlock != null) {
            brushBlock.hide();
            brushBlock.empty();
            brushBlock = null;
        }
    }

    // Показывает кисть в указанном блоке
    public virtual void ShowBrush(Coord blockCoord, Mesh mesh, Vector3 offset) {
        var block = GetBlock(blockCoord);
        block.fill(brush, false, offset, transform.rotation);
        brushBlock = block;
        brush.GetComponent<MeshFilter>().mesh = mesh;
        brush.transform.localScale = new Vector3(blockSize, blockSize, blockSize);
    }

    // Возвращает блок по координатам
    public Block GetBlock(int x, int y, int z) {
        return GetBlock(new Coord(x, y, z));
    }

    // Возвращает блок по координатам
    public Block GetBlock(Coord coord) {

        if(coord.x >= size || coord.y >= size || coord.z >= size) {
            return null;
        }

        return blocks[coord.x][coord.y][coord.z];
    }

    // Возвращает координаты блока
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

    // Возвращает координаты блока с указанным GameObject'ом
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

    // Убирает указанный GameObject из редактора
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

    // Добавляет GameObject по указанным координатам
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

    // Каждый тик, когда что-то находится в редакторе
    // Обрабатывает добавление объектов в редактор
    protected virtual void OnTriggerStay(Collider other) {

        // Не редактируем
        if (!editable) {
            HideBrush();
            return;
        }

        var otherBlock = other.gameObject;
        Vector3 offset;
        List<Block> closestAffectedBlocks;
        bool otherIsActive;

        // Проверяем валидность объекта и находим ближайший блок, в который можно его поставить
        var closestBlockCoord = GetClosestBlockCoord(otherBlock, out offset, out closestAffectedBlocks, out otherIsActive);

        // Блок найден
        if (closestBlockCoord != null) {

            // Если он в руке игрока, показываем кисть
            if (otherIsActive) {
                ShowBrush(closestBlockCoord, otherBlock.GetComponent<MeshFilter>().mesh, offset);
            }
            else {
                // Иначе ставим объект
                AddBlock(closestBlockCoord, otherBlock, offset, closestAffectedBlocks);
            }
        }
    }

    // Когда что-то покидает редактор
    // Прячет кисть
    protected virtual void OnTriggerExit(Collider other) {
        HideBrush();
    }

    // Вовзращает координаты ближайшего блока, а также оффсет, блоки, на которые будет наложен GameObject и находится ли он в руке игрока
    protected Coord GetClosestBlockCoord(GameObject otherBlock, out Vector3 offset, out List<Block> closestAffectedBlocks, out bool isActive) {
        offset = new Vector3();
        closestAffectedBlocks = null;
        isActive = false;

        var interactible = otherBlock.GetComponent<Interactible>();

        // Это не объект для редактора
        if (!interactible) {
            return null;
        }

        isActive = interactible.isActive;
        if (blocksList.Contains(otherBlock)) {

            if (isActive) {

                // Игрок взял объект, который ранее был поставлен в редактор
                RemoveBlock(otherBlock);
            }
            else {

                // Этот объект уже поставлен в редактор и его никто не трогает
                return null;
            }
        }

        // Мы уже знаем, что это объект для редактора, так что можно убрать кисть из старой позиции
        HideBrush();

        var minDist = Mathf.Infinity;
        Coord closestBlockCoord = null;

        // Рассчитываем оффсет объекта
        var otherCollider = otherBlock.GetComponent<BoxCollider>();
        offset = new Vector3(otherCollider.size.x, otherCollider.size.y, otherCollider.size.z) * blockSize / 2;
        var otherPosition = otherBlock.transform.position - transform.rotation * offset;

        // Находим ближайший незаполенный блок
        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                for (int z = 0; z < size; z++) {
                    var blockCoord = new Coord(x, y, z);
                    List<Block> affectedBlocks;

                    // Проверяем валидность блока
                    var block = GetValidBlock(blockCoord, otherBlock.GetComponent<BoxCollider>().size, out affectedBlocks);
                    if (block == null) {
                        continue;
                    }

                    // Проверяем дальность блока
                    var dist = Vector3.Distance(otherPosition, block.position);
                    if (dist < blockSize * maxBlockDistance && dist < minDist) {
                        closestBlockCoord = new Coord(x, y, z);
                        minDist = dist;
                        closestAffectedBlocks = affectedBlocks;
                    }
                }
            }
        }

        return closestBlockCoord;
    }

    // Возвращает ближайший валидный блок и блоки, на которые будет наложен GameObject
    Block GetValidBlock(Coord blockCoord, Vector3 otherBlockSize, out List<Block> affectedBlocks) {
        var block = GetBlock(blockCoord);
        var isValid = false;
        
        // Блок на нижнем этаже можно заполнить всегда
        if (blockCoord.y == 0) {
            isValid = true;
        }
        else {
            // Проверяем, не будет ли блок висеть в воздухе
            var connectedBlocks = GetConnectedBlocks(blockCoord.x, blockCoord.y, blockCoord.z);
            foreach (Block connectedBlock in connectedBlocks) {
                if (!connectedBlock.isEmpty && !connectedBlock.sameAs(brush)) {
                    isValid = true;
                }
            }
        }

        affectedBlocks = new List<Block>();

        // Проверяем, не пересечется ли объект с другими заполненными блоками и сразу собираем все блоки пересеченные блоки в массив
        if (isValid) {
            for (int i = 1; i <= 3; i++) {
                isValid = CheckAndAppendAffectedBlocks(affectedBlocks, blockCoord, otherBlockSize, i);
                if (!isValid) break;
            }
        }

        isValid = isValid && (block.isEmpty || block.sameAs(brush));
        
        return isValid ? block : null;
    }

    // Возвращает все блоки, рядом с текущим
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

    // Добавляет блоки, на которые будет наложен GameObject по одной стороне
    bool CheckAndAppendAffectedBlocks(List<Block> affectedBlocks, Coord blockCoord, Vector3 otherBlockSize, int dimension) {

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

}
