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

    // Кол-во клеток по сторонам
    public int sizeX = 10;
    public int sizeY = 10;
    public int sizeZ = 10;

    // Размер сетки
    [HideInInspector]
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

    // Позволяет блокам выходить за пределы сетки
    public bool allowOutOfBoundsPlacement = false;

    // Включает дебаг сетку
    public bool debugGridEnabled = true;

    // Префаб дебаг сетки
    public GameObject DebugGridPrefab;


    /* События Unity */

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

                    var gridPrefab = debugGridEnabled ? Instantiate(DebugGridPrefab) : new GameObject();
                    if (debugGridEnabled) {
                        var gridMesh = gridPrefab.transform.GetChild(0);
                        gridMesh.localScale = blockSize;
                        gridMesh.localPosition = blockSize / 2;
                    }

                    var block = new Block(
                        position + Vector3.Scale(new Vector3(x, y, z), blockSize),
                        new Coord(x, y, z),
                        transform.parent,
                        gridPrefab
                    );

                    axis.Add(block);
                }

                dimension.Add(axis);
            }

            blocks.Add(dimension);
        }
    }

    // Каждый тик, когда что-то находится в редакторе
    // Обрабатывает добавление объектов в редактор
    protected virtual void OnTriggerStay(Collider other) {

        // Не редактируем
        // TODO: убрать возможность забирать блоки
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

            // Считаем поворот объекта
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


    /* Добавление/удаление объектов */

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
        WriteDebug(obj);

        var block = GetBlock(blockCoord);
        var offset = CalculateOffset(obj);

        blocksList.Add(obj);
        block.fill(obj, true, offset, rotation, affectedBlocks);
    }


    /* Работа с блоками */

    // Возвращает блок по координатам
    public Block GetBlock(int x, int y, int z) {
        return GetBlock(new Coord(x, y, z));
    }

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


    /* Расставление объектов */

    // Вовзращает координаты ближайшего валидного блока, а также блоки, на которые будет наложен GameObject и находится ли он в руке игрока
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

                    // Проверяем валидность блока, находим растояние до блока и задетые блоки
                    var blockCoord = new Coord(x, y, z);
                    var block = GetBlock(blockCoord);
                    var dist = Vector3.Distance(objPosition, block.position);;
                    List<Block> affectedBlocks = null;
                    
                    if (!BlockIsValid(block, dist) || !ObjectFits(blockCoord, obj, out affectedBlocks)) {
                        continue;
                    }

                    if (dist < minDist) {
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
    protected bool BlockIsValid(Block block, float distance) {
        return BlockIsEmpty(block) && distance < blockSize.magnitude * maxBlockDistance;
    }

    // Пуст ли блок
    protected bool BlockIsEmpty(Block block) {
        return block.isEmpty || block.has(brush);
    }

    // Проверяет, не пересечется ли объект с другими заполненными блоками и собирает все пересеченные блоки в массив
    // Также проверяет, прилегает ли блок к ранее поставленному объекту
    bool ObjectFits(Coord blockCoord, GameObject obj, out List<Block> affectedBlocks) {
        affectedBlocks = new List<Block>();
        var blockReach = CalculateBlockReach(obj, blockCoord);
        var isConnected = false;
        for (int x = blockReach.x; x >= blockCoord.x; x--) {
            for (int y = blockReach.y; y >= blockCoord.y; y--) {
                for (int z = blockReach.z; z >= blockCoord.z; z--) {
                    var affectedBlock = GetBlock(x, y, z);

                    if (allowOutOfBoundsPlacement && affectedBlock == null) {
                        continue;
                    }

                    if (affectedBlock == null || !affectedBlock.isEmpty) {
                        affectedBlocks = null;
                        return false;
                    }

                    isConnected = isConnected || BlockIsConnected(x, y, z);
                    if (blockCoord.x == x && blockCoord.y == y && blockCoord.z == z) continue;
                    affectedBlocks.Add(affectedBlock);
                }
            }
        }
        if (!isConnected) {
            affectedBlocks = null;
        }
        return isConnected;
    }

    // Проверяет, находится ли блок рядом с хотя бы одним уже заполненным
    bool BlockIsConnected(int x, int y, int z) {
        if (y == 0) return true;

        if (x > 0 && !BlockIsEmpty(blocks[x - 1][y][z])) {
            return true;
        }

        if (x < size.x - 1 && !BlockIsEmpty(blocks[x + 1][y][z])) {
            return true;
        }

        if (y > 0 && !BlockIsEmpty(blocks[x][y - 1][z])) {
            return true;
        }

        if (y < size.y - 1 && !BlockIsEmpty(blocks[x][y + 1][z])) {
            return true;
        }

        if (z > 0 && !BlockIsEmpty(blocks[x][y][z - 1])) {
            return true;
        }

        if (z < size.z - 1 && !BlockIsEmpty(blocks[x][y][z + 1])) {
            return true;
        }

        return false;
    }


    /* Размеры с объекта */

    // Считает поворот объекта
    protected virtual Quaternion CalculateRotation(GameObject obj) {
        Quaternion appliedRotation = Quaternion.identity;
        var identity = obj.GetComponent<ObjectIdentity>();
        return identity ? identity.GetRotation() :  Quaternion.identity;
    }

    // Считает размер объекта
    protected Vector3 CalculateSize(GameObject obj) {
        var collider = obj.GetComponent<BoxCollider>();
        var objSize = CalculateRotation(obj) * Vector3.Scale((collider ? collider.size : new Vector3(1, 1, 1)), obj.transform.localScale);
        return new Vector3(Mathf.Abs(objSize.x), Mathf.Abs(objSize.y), Mathf.Abs(objSize.z));
    }

    // Возвращает координаты самого дальнего от переданного блока, который будет занят объектом
    protected Coord CalculateBlockReach(GameObject obj, Coord blockCoord) {
        var objSize = CalculateSize(obj);
        return new Coord(
            blockCoord.x + Mathf.RoundToInt(objSize.x / blockSize.x) - 1,
            blockCoord.y + Mathf.RoundToInt(objSize.y / blockSize.y) - 1,
            blockCoord.z + Mathf.RoundToInt(objSize.z / blockSize.z) - 1
        );
    }

    // Возвращает размер объекта с округлением до ближайшего блока
    protected Vector3 CalculateMinFitSize(GameObject obj) {
        var objSize = CalculateSize(obj);
        return new Vector3(
            Mathf.RoundToInt(objSize.x / blockSize.x) * blockSize.x,
            Mathf.RoundToInt(objSize.y / blockSize.y) * blockSize.y,
            Mathf.RoundToInt(objSize.z / blockSize.z) * blockSize.z
        );
    }

    // Считает необходимый сдвиг объекта, чтобы он визуально умещался в предоставленных ему блоках
    protected Vector3 CalculateOffset(GameObject obj) {
        var collider = obj.GetComponent<BoxCollider>();
        var offset = Vector3.Scale(CalculateRotation(obj) * collider.center, obj.transform.localScale);
        return CalculateMinFitSize(obj) / 2 - offset;
    }


    /* Дебаг */

    protected void WriteDebug(GameObject obj) {
        if (!debugGridEnabled) return;
        var s1 = CalculateBlockReach(obj, new Coord(0, 0, 0));
        var s2 = CalculateSize(obj);
        var s3 = CalculateMinFitSize(obj);
        var s4 = CalculateRotation(obj);
        Debug.LogFormat("{3} | {4} | {5} | {0}, {1}, {2}", s1.x, s1.y, s1.z, s2, s3, s4);
    }

}
