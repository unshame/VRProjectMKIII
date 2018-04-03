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

    // Кол-во клеток сторонам
    public int sizeX = 10;
    public int sizeY = 10;
    public int sizeZ = 10;

    // Размер сетки
    public Coord size;
    
    // Размер блока (рассчитывается из size)
    [HideInInspector]
    public Vector3 blockSize;

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
    public int maxBlockDistance = 2;

    // Инициализирует кисть, создает блоки
    protected virtual void Start() {
        size = new Coord(sizeX, sizeY, sizeZ);

        blockSize = new Vector3(
            transform.localScale.x / size.x,
            transform.localScale.y / size.y,
            transform.localScale.z / size.z
        );

        brush = Instantiate(brush, transform.parent);
        brush.GetComponent<Renderer>().material = brushMaterial;
        brush.GetComponent<Rigidbody>().isKinematic = true;
        brush.GetComponent<Renderer>().enabled = false;
        brush.GetComponent<Collider>().enabled = false;
        brush.transform.localScale = blockSize;

        var offset = transform.localScale / 2;
        var position = transform.position - offset;

        for (int x = 0; x < size.x; x++) {
            var dimension = new List<List<Block>>();

            for (int y = 0; y < size.y; y++) {
                var axis = new List<Block>();

                for (int z = 0; z < size.z; z++) {

                    var block = new Block(
                        position + Vector3.Scale(new Vector3(x, y, z), blockSize),
                        new Coord(x, y, z),
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
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                for (int z = 0; z < size.z; z++) {
                    blocks[x][y][z].updatePosition();
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
    public virtual void ShowBrush(Coord blockCoord, GameObject obj, Quaternion rotation) {
        var block = GetBlock(blockCoord);
        var offset = CalculateOffset(obj);
        block.fill(brush, false, offset, rotation * obj.transform.localRotation);
        brushBlock = block;
        brush.GetComponent<MeshFilter>().mesh = obj.GetComponent<MeshFilter>().mesh;
        brush.transform.localScale = obj.transform.localScale;
    }

    // Возвращает блок по координатам
    public Block GetBlock(int x, int y, int z) {
        return GetBlock(new Coord(x, y, z));
    }

    // Возвращает блок по координатам
    public Block GetBlock(Coord coord) {

        if(coord.x < 0 || coord.x >= size.x || coord.y < 0 || coord.y >= size.y || coord.z < 0 || coord.z >= size.z) {
            return null;
        }

        return blocks[coord.x][coord.y][coord.z];
    }

    // Возвращает координаты блока с указанным GameObject'ом
    public Coord GetObjectCoord(GameObject obj) {
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                for (int z = 0; z < size.z; z++) {
                    if (blocks[x][y][z].has(obj)) {
                        return new Coord(x, y, z);
                    };

                }
            }
        }
        return null;
    }

    // Убирает указанный GameObject из редактора
    public virtual void RemoveObject(GameObject obj) {

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                for (int z = 0; z < size.z; z++) {
                    var block = blocks[x][y][z];
                    block.empty(obj);
                }
            }
        }

        blocksList.Remove(obj);
    }

    // Добавляет GameObject по указанным координатам
    public virtual void AddObject(Coord blockCoord, GameObject obj, List<Block> affectedBlocks, Quaternion rotation) {
        var block = GetBlock(blockCoord);
        var offset = CalculateOffset(obj);

        blocksList.Add(obj);
        block.fill(obj, true, offset, rotation, affectedBlocks);
    }

    // Каждый тик, когда что-то находится в редакторе
    // Обрабатывает добавление объектов в редактор
    protected virtual void OnTriggerStay(Collider other) {

        // Не редактируем
        if (!editable) {
            HideBrush();
            return;
        }

        var obj = other.gameObject;
        List<Block> closestAffectedBlocks;
        bool objIsActive;


        // Проверяем валидность объекта и находим ближайший блок, в который можно его поставить
        var closestBlockCoord = GetClosestBlockCoord(obj, out closestAffectedBlocks, out objIsActive);

        // Блок найден
        if (closestBlockCoord != null) {

            var appliedRotation = CalculateRotation(obj);

            // Если он в руке игрока, показываем кисть
            if (objIsActive) {
                ShowBrush(closestBlockCoord, obj, appliedRotation);
            }
            else {
                // Иначе ставим объект
                AddObject(closestBlockCoord, obj, closestAffectedBlocks, appliedRotation);
            }
        }
    }

    // Когда что-то покидает редактор
    // Прячет кисть
    protected virtual void OnTriggerExit(Collider other) {
        HideBrush();
    }

    protected virtual Quaternion CalculateRotation(GameObject obj) {
        Quaternion appliedRotation = Quaternion.identity;

        var identity = obj.GetComponent<ObjectIdentity>();
        if (!identity || !identity.CanRotate()) return appliedRotation;
        
        var rotationAxis = identity.rotationAxis;
        var rotationAngle = identity.rotationAngle;
        var rotationIndex = identity.GetRotationIndex();

        if (rotationAxis == 0) {
            appliedRotation = Quaternion.Euler(rotationAngle * rotationIndex, 0, 0);
        }
        else if(rotationAxis == 1) {
            appliedRotation = Quaternion.Euler(0, rotationAngle * rotationIndex, 0);
        }
        else if (rotationAxis == 2) {
            appliedRotation = Quaternion.Euler(0, 0, rotationAngle * rotationIndex);
        }

        return appliedRotation;
    }

    protected Vector3 CalculateSize(GameObject obj) {
        var collider = obj.GetComponent<BoxCollider>();
        return Vector3.Scale((collider ? collider.size : new Vector3(1, 1, 1)), obj.transform.localScale);
    }

    protected Coord CalculateRequiredBlocks(GameObject obj) {
        var objSize = CalculateSize(obj);
        return new Coord(
            Mathf.CeilToInt(objSize.x / blockSize.x),
            Mathf.CeilToInt(objSize.y / blockSize.y),
            Mathf.CeilToInt(objSize.z / blockSize.z)
        );
    }

    protected Vector3 CalculateMinFitSize(GameObject obj) {
        var requiredBlocks = CalculateRequiredBlocks(obj);
        return new Vector3(
            requiredBlocks.x * blockSize.x,
            requiredBlocks.y * blockSize.y,
            requiredBlocks.z * blockSize.z
        );
    }

    protected Vector3 CalculateOffset(GameObject obj) {
        var objIdentity = obj.GetComponent<ObjectIdentity>();
        var identityOffset = objIdentity ? objIdentity.offset : Vector3.zero;
        return CalculateMinFitSize(obj) / 2 + Vector3.Scale(identityOffset, obj.transform.localScale);
    }

    // Вовзращает координаты ближайшего блока, а также оффсет, блоки, на которые будет наложен GameObject и находится ли он в руке игрока
    protected Coord GetClosestBlockCoord(GameObject obj, out List<Block> closestAffectedBlocks, out bool isActive) {
        closestAffectedBlocks = null;
        isActive = false;

        var interactible = obj.GetComponent<Interactible>();

        // Это не объект для редактора
        if (!interactible) {
            return null;
        }

        isActive = interactible.isActive;
        if (blocksList.Contains(obj)) {

            if (isActive) {

                // Игрок взял объект, который ранее был поставлен в редактор
                RemoveObject(obj);
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
        var offset = CalculateOffset(obj);
        var objPosition = obj.transform.position - transform.rotation * offset;

        // Находим ближайший незаполенный блок
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                for (int z = 0; z < size.z; z++) {
                    var blockCoord = new Coord(x, y, z);
                    List<Block> affectedBlocks;

                    // Проверяем валидность блока
                    var block = GetValidBlock(blockCoord, obj, out affectedBlocks);
                    if (block == null) {
                        continue;
                    }

                    // Проверяем дальность блока
                    var dist = Vector3.Distance(objPosition, block.position);
                    if (dist < blockSize.magnitude * maxBlockDistance && dist < minDist) {
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
    Block GetValidBlock(Coord blockCoord, GameObject obj, out List<Block> affectedBlocks) {
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
                if (!connectedBlock.isEmpty && !connectedBlock.has(brush)) {
                    isValid = true;
                }
            }
        }

        affectedBlocks = new List<Block>();

        // Проверяем, не пересечется ли объект с другими заполненными блоками и сразу собираем все блоки пересеченные блоки в массив
        if (isValid) {
            for (int i = 1; i <= 3; i++) {
                isValid = CheckAndAppendAffectedBlocks(affectedBlocks, blockCoord, obj, i);
                if (!isValid) break;
            }
        }

        isValid = isValid && (block.isEmpty || block.has(brush));
        
        return isValid ? block : null;
    }

    // Добавляет блоки, на которые будет наложен GameObject по одной стороне
    bool CheckAndAppendAffectedBlocks(List<Block> affectedBlocks, Coord blockCoord, GameObject obj, int dimension) {

        var requiredBlocks = CalculateRequiredBlocks(obj);
        var sizeCoord = new Coord(
            blockCoord.x + requiredBlocks.x - 1, 
            blockCoord.y + requiredBlocks.y - 1, 
            blockCoord.z + requiredBlocks.z - 1
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

    // Возвращает все блоки, рядом с текущим
    List<Block> GetConnectedBlocks(int x, int y, int z) {
        List<Block> connectedBlocks = new List<Block>();

        if (x > 0) {
            connectedBlocks.Add(blocks[x - 1][y][z]);
        }

        if (x < size.x - 1) {
            connectedBlocks.Add(blocks[x + 1][y][z]);
        }

        if (y > 0) {
            connectedBlocks.Add(blocks[x][y - 1][z]);
        }

        if (y < size.y - 1) {
            connectedBlocks.Add(blocks[x][y + 1][z]);
        }

        if (z > 0) {
            connectedBlocks.Add(blocks[x][y][z - 1]);
        }

        if (z < size.z - 1) {
            connectedBlocks.Add(blocks[x][y][z + 1]);
        }

        return connectedBlocks;
    }

}
