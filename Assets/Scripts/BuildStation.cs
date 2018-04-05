using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Редактор здания
public class BuildStation : MonoBehaviour {

    // Размер сетки
    public Vector3i size = new Vector3i(10, 10, 10);
    
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
    public Block[][][] blocks;

    // Массив всех GameObject'ов в редакторе
    [HideInInspector]
    public List<GameObject> objList = new List<GameObject>();

    // Можно ли изменять контент редактора в игре
    public bool editable = true;

    // Максимальная удаленность валидного блока от объекта, который планируется туда поставить (дистанция в блоках)
    public int maxBlockDistance = 2;

    // Позволяет блокам выходить за пределы сетки
    public bool allowOutOfBoundsPlacement = false;

    // Показывать ли кисть
    public bool shouldShowBrush = true;

    // Включает дебаг сетку
    public bool debugGridEnabled = false;

    // Префаб дебаг сетки
    public GameObject DebugGridPrefab;


    /* События Unity */

    // Инициализирует кисть, создает блоки
    protected virtual void Start() {
        blockSize = VectorUtils.Divide(transform.localScale, size);

        // Инстанциируем кисть
        brush = Instantiate(brush, transform.parent);
        brush.GetComponent<Renderer>().material = brushMaterial;
        brush.GetComponent<Renderer>().enabled = false;
        brush.GetComponent<Collider>().enabled = false;
        brush.transform.localScale = blockSize;

        var offset = transform.localScale / 2;
        var position = transform.position - offset;

        // Создаем блоки
        blocks = new Block[size.x][][];
        for (int x = 0; x < size.x; x++) {
            blocks[x] = new Block[size.y][];

            for (int y = 0; y < size.y; y++) {
                blocks[x][y] = new Block[size.z];

                for (int z = 0; z < size.z; z++) {

                    blocks[x][y][z] = new Block(
                        position + Vector3.Scale(new Vector3(x, y, z), blockSize),
                        new Vector3i(x, y, z),
                        transform.parent,
                        CreateBlockAnchor()
                    );
                }
            }
        }
    }

    protected virtual void Update() { }

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
        Block[] closestAffectedBlocks;
        bool objIsActive;

        // Проверяем валидность объекта и находим ближайший блок, в который можно его поставить
        var closestBlockCoord = GetClosestBlockCoord(obj, out closestAffectedBlocks, out objIsActive);

        // Блок найден
        if (closestBlockCoord != -Vector3i.one) {

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
    public virtual void ShowBrush(Vector3i blockCoord, GameObject obj, Quaternion rotation) {
        if (!shouldShowBrush) return;

        var block = GetBlock(blockCoord);
        var offset = CalculateOffset(obj);

        // Заполняем блоки кистью
        block.fill(brush, false, offset, rotation * obj.transform.localRotation);
        brushBlock = block;

        // Устанавливаем меш и масштаб кисти
        brush.GetComponent<MeshFilter>().mesh = obj.GetComponent<MeshFilter>().mesh;
        brush.transform.localScale = obj.transform.localScale;
    }

    // Убирает указанный GameObject из редактора
    public virtual void RemoveObject(GameObject obj) {

        if((obj != brush || brushBlock == null) && !objList.Contains(obj)) return;

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                for (int z = 0; z < size.z; z++) {
                    var block = blocks[x][y][z];
                    block.empty(obj);
                }
            }
        }

        objList.Remove(obj);
    }

    // Добавляет GameObject по указанным координатам
    public virtual void AddObject(Vector3i blockCoord, GameObject obj, Block[] affectedBlocks, Quaternion rotation) {
        WriteDebug(obj);

        var block = GetBlock(blockCoord);
        var offset = CalculateOffset(obj);

        // Заполняем массив объектов и блоки объектом
        objList.Add(obj);
        block.fill(obj, true, offset, rotation, affectedBlocks);
    }


    /* Работа с блоками */

    // Создает GameObject для блока
    protected GameObject CreateBlockAnchor() {

        // Дебаг отображение блока или пустой объект
        var blockAnchor = debugGridEnabled ? Instantiate(DebugGridPrefab) : new GameObject();

        // Устанавливаем размер и позицию дебаг блока
        if (debugGridEnabled) {
            var gridMesh = blockAnchor.transform.GetChild(0);
            gridMesh.localScale = blockSize;
            gridMesh.localPosition = blockSize / 2;
        }

        // Добавляем объект, который будет держать добавляемые объекты на месте
        var childObject = new GameObject();
        childObject.transform.parent = blockAnchor.transform;
        childObject.AddComponent<BlockHolder>();

        return blockAnchor;
    }

    // Возвращает блок по координатам
    public Block GetBlock(int x, int y, int z) {
        return GetBlock(new Vector3i(x, y, z));
    }

    public Block GetBlock(Vector3i coord) {

        if(coord.x < 0 || coord.x >= size.x || coord.y < 0 || coord.y >= size.y || coord.z < 0 || coord.z >= size.z) {
            return null;
        }

        return blocks[coord.x][coord.y][coord.z];
    }

    // Возвращает координаты блока с указанным GameObject'ом или -1 вектор
    public Vector3i GetObjectCoord(GameObject obj) {
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                for (int z = 0; z < size.z; z++) {
                    if (blocks[x][y][z].has(obj)) {
                        return new Vector3i(x, y, z);
                    };

                }
            }
        }
        return -Vector3i.one;
    }


    /* Расставление объектов */

    // Вовзращает координаты ближайшего валидного блока, а также блоки, на которые будет наложен GameObject и находится ли он в руке игрока
    // Возращает -1 вектор, если такого блока нет
    protected Vector3i GetClosestBlockCoord(GameObject obj, out Block[] closestAffectedBlocks, out bool isActive) {
        closestAffectedBlocks = null;
        isActive = false;

        var interactible = obj.GetComponent<Interactible>();

        // Это не объект для редактора
        if (!interactible) {
            return -Vector3i.one;
        }

        isActive = interactible.isActive;
        
        // Объект уже в редакторе
        if (objList.Contains(obj)) {

            if (isActive) {

                // Игрок взял объект, который ранее был поставлен в редактор
                RemoveObject(obj);
            }
            else {

                // Этот объект уже поставлен в редактор и его никто не трогает
                return -Vector3i.one;
            }
        }

        // Мы уже знаем, что это объект для редактора, так что можно убрать кисть из старой позиции
        HideBrush();

        var minDist = Mathf.Infinity;
        Vector3i closestBlockCoord = -Vector3i.one;

        // Рассчитываем оффсет объекта
        var offset = CalculateOffset(obj);
        var objPosition = obj.transform.position - transform.rotation * offset;

        // Находим ближайший незаполенный блок
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                for (int z = 0; z < size.z; z++) {

                    // Проверяем валидность блока, находим растояние до блока и задетые блоки
                    var blockCoord = new Vector3i(x, y, z);
                    var block = GetBlock(blockCoord);
                    var dist = Vector3.Distance(objPosition, block.position);;
                    Block[] affectedBlocks = null;
                    
                    if (!BlockIsValid(block, dist) || !ObjectFits(blockCoord, obj, out affectedBlocks)) {
                        continue;
                    }

                    // Находим ближайший блок
                    if (dist < minDist) {
                        closestBlockCoord = new Vector3i(x, y, z);
                        minDist = dist;
                        closestAffectedBlocks = affectedBlocks;
                    }
                }
            }
        }

        return closestBlockCoord;
    }

    // Проверяет удаленность и заполненность блока
    protected bool BlockIsValid(Block block, float distance) {
        return BlockIsEmpty(block) && distance < blockSize.magnitude * maxBlockDistance;
    }

    // Пуст ли блок (несуществующие блоки считаются здесь пустыми)
    protected bool BlockIsEmpty(Block block) {
        return block == null || (block.isEmpty || block.has(brush));
    }

    // Проверяет, не пересечется ли объект с другими заполненными блоками и собирает все пересеченные блоки в массив
    // Также проверяет, прилегает ли блок к ранее поставленному объекту
    bool ObjectFits(Vector3i blockCoord, GameObject obj, out Block[] affectedBlocks) {
        var blockMagnitude = CalculateBlockMagnitude(obj);
        
        // Самый дальний блок, который будет занят объектом
        var blockReach = blockCoord + blockMagnitude - Vector3i.one;

        affectedBlocks = new Block[blockMagnitude.x * blockMagnitude.y * blockMagnitude.z];
        var i = 0;
        var isConnected = false;

        for (int x = blockReach.x; x >= blockCoord.x; x--) {
            for (int y = blockReach.y; y >= blockCoord.y; y--) {
                for (int z = blockReach.z; z >= blockCoord.z; z--) {
                    var affectedBlock = GetBlock(x, y, z);

                    // Если объектам разрешено выходить за пределы редактора, пропускаем несуществующие блоки
                    if (allowOutOfBoundsPlacement && affectedBlock == null) {
                        continue;
                    }

                    // Объект не удастся разместить, т.к. блок занят или не существует
                    if (affectedBlock == null || !affectedBlock.isEmpty) {
                        affectedBlocks = null;
                        return false;
                    }

                    // Проверяем прилегающие блоки
                    isConnected = isConnected || BlockIsConnected(x, y, z);

                    // Пропускаем текущий блок
                    if (blockCoord.x == x && blockCoord.y == y && blockCoord.z == z) continue;

                    // Добавляем блок в массив блоков, занятых объектом
                    affectedBlocks[i] = affectedBlock;

                    i++;
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

        var coord = new Vector3i(x, y, z);

        // Проверяем пустоту ближаших блоков по каждой оси
        for(int i = 0; i < 3; i++) {
            var increment = Vector3i.zero;
            increment[i] = 1;

            if (!BlockIsEmpty(GetBlock(coord + increment)) || !BlockIsEmpty(GetBlock(coord - increment))) {
                return true;
            }
        }

        return false;
    }


    /* Размеры с объекта */

    // Считает поворот объекта
    // Учитывается только поворот в identity блока, реальный поворот применяется позже только визуально
    protected virtual Quaternion CalculateRotation(GameObject obj) {
        Quaternion appliedRotation = Quaternion.identity;
        var identity = obj.GetComponent<ObjectIdentity>();
        return identity ? identity.GetRotation() :  Quaternion.identity;
    }

    // Считает размер объекта
    // Размер коллайдера * масштаб, повернутый на поворот identity объекта
    protected Vector3 CalculateSize(GameObject obj) {
        var collider = obj.GetComponent<BoxCollider>();
        var objSize = CalculateRotation(obj) * Vector3.Scale((collider ? collider.size : Vector3.one), obj.transform.localScale);
        return new Vector3(Mathf.Abs(objSize.x), Mathf.Abs(objSize.y), Mathf.Abs(objSize.z));
    }

    // Считает число блоков между двумя крайними блоками, которые будет занимать объект (минимум один блок)
    protected Vector3i CalculateBlockMagnitude(GameObject obj) {
        var objSize = CalculateSize(obj);
        return VectorUtils.Max(VectorUtils.RoundToInt(VectorUtils.Divide(objSize, blockSize)), Vector3i.one);
    }

    // Возвращает размер объекта с округлением до ближайшего блока (минимум один блок)
    protected Vector3 CalculateMinFitSize(GameObject obj) {
        var objSize = CalculateSize(obj);
        return Vector3.Scale(VectorUtils.Max(VectorUtils.RoundToInt(VectorUtils.Divide(objSize, blockSize)), Vector3i.one), blockSize);
    }

    // Считает необходимый сдвиг объекта, чтобы он визуально умещался в предоставленных ему блоках
    // По умолчанию блок находится в дальнем углу базового блока, 
    // мы его сдвигаем до центра занимаемых им блоков - оффсет коллайдера + оффсет identity
    protected Vector3 CalculateOffset(GameObject obj) {
        var collider = obj.GetComponent<BoxCollider>();
        var objIdentity = obj.GetComponent<ObjectIdentity>();
        var identityOffset = objIdentity ? objIdentity.offset : Vector3.zero;
        var offset = CalculateRotation(obj) * Vector3.Scale(identityOffset - collider.center, obj.transform.localScale);
        return CalculateMinFitSize(obj) / 2 + offset;
    }


    /* Дебаг */

    protected void WriteDebug(GameObject obj) {
        if (!debugGridEnabled) return;
        var s1 = CalculateBlockMagnitude(obj);
        var s2 = CalculateSize(obj);
        var s3 = CalculateMinFitSize(obj);
        var s4 = CalculateRotation(obj);
        Debug.LogFormat("{0} | {1} | {2} | {3}", s1, s2, s3, s4);
    }

}
