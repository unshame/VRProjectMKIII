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

    // 3D массив блоков
    [HideInInspector]
    public Block[][][] blocks;

    // Массив всех GameObject'ов в редакторе
    [HideInInspector]
    public List<GameObject> objList = new List<GameObject>();


    // Показывать ли кисть
    public bool shouldShowBrush = true;

    // Префаб, а после инициализации - инстанция кисти-блока (полупрозрачный блок)
    public GameObject brush;

    // Материал кисти
    public Material brushMaterial;

    // Блок, в котором находится кисть
    protected Block brushBlock = null;

    // Для какого объекта показана кисть
    protected GameObject brushShownFor = null;


    // Можно ли изменять контент редактора в игре
    public bool editable;

    // Максимальная удаленность валидного блока от объекта, который планируется туда поставить (дистанция в блоках)
    public int maxBlockDistance = 2;

    // Максимальный выступ объекта за блок, после которого считается, что объект занимает блок
    public float maxObjectProtrusion = 0.1f;
    
    // Интервал проверки позиции объекта в поле
    public float objectUpdateInterval = 0.1f;


    // Когда объект был обновлен в последний раз
    protected float sinceLastUpdate = 0f;

    // Очередь объектов на обработку
    protected List<GameObject> updateQueue = new List<GameObject>();


    // Включает дебаг сетку
    public bool debugGridEnabled = false;

    // Префаб дебаг сетки
    public GameObject DebugGridPrefab;


    /* События Unity */

    // Считает размер блоков
    protected virtual void Awake() {
        blockSize = VectorUtils.Divide(transform.localScale, size);  
    }

    // Инициализирует кисть, создает блоки
    protected virtual void Start() {
        // Инстанциируем кисть
        brush = Instantiate(brush, transform.parent);
        brush.GetComponent<Renderer>().material = brushMaterial;
        brush.GetComponent<Renderer>().enabled = false;
        brush.GetComponent<Collider>().enabled = false;
        brush.transform.localScale = blockSize;

        var blockHolder = new GameObject {
            name = "BlockHolder"            
        };
        blockHolder.transform.parent = transform.parent;

        var offset = transform.localScale / 2;
        var position = transform.position - offset;

        // Создаем блоки
        blocks = new Block[size.x][][];
        for (int x = 0; x < size.x; x++) {
            blocks[x] = new Block[size.y][];

            for (int y = 0; y < size.y; y++) {
                blocks[x][y] = new Block[size.z];

                for (int z = 0; z < size.z; z++) {

                    var blockCoord = new Vector3i(x, y, z);

                    blocks[x][y][z] = new Block(
                        position + Vector3.Scale(blockCoord, blockSize),
                        blockCoord,
                        blockHolder.transform,
                        CreateBlockAnchor()
                    );
                }
            }
        }
    }

    // Обрабатывает объекты в очереди
    protected virtual void Update() {

        sinceLastUpdate += Time.deltaTime;

        if (!editable) {
            HideBrush();
            return;
        }

        if (sinceLastUpdate >= objectUpdateInterval && updateQueue.Count > 0) {
            sinceLastUpdate = 0;

            PlaceObject(DequeueObject());
        }
    }

    // Каждый тик, когда что-то находится в редакторе
    // Обрабатывает добавление объектов в редактор
    protected virtual void OnTriggerStay(Collider other) {

        // Не редактируем
        if (!editable) {
            return;
        }

        ProcessObject(other.gameObject);
    }

    // Когда что-то покидает редактор
    // Прячет кисть, удаляет объект из очереди на добавление
    protected virtual void OnTriggerExit(Collider other) {
        DequeueObject(other.gameObject);
        HideBrush();
    }


    /* Добавление/удаление объектов */

    // Прячет кисть
    public virtual void HideBrush() {

        if (brushBlock != null) {
            brushBlock.hide();
            brushBlock.empty();
            brushBlock = null;
            brushShownFor = null;
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
        brushShownFor = obj;

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

    // Отключает редактирование
    public virtual void Lock() {
        for(int i = 0; i < objList.Count; i++) {
            objList[i].GetComponent<Interactible>().isLocked = true;
        }
        editable = false;
    }

    // Отключает редактирование на x секунд
    public virtual IEnumerator LockFor(float seconds) {
        Lock();
        yield return new WaitForSeconds(seconds);
        Unlock();
    }

    // Включает редактирование
    public virtual void Unlock() {
        for (int i = 0; i < objList.Count; i++) {
            objList[i].GetComponent<Interactible>().isLocked = false;
        }
        editable = true;
    }

    // Убирает все объекты
    public virtual void Clear() {
        if (!editable) return;

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                for (int z = 0; z < size.z; z++) {
                    blocks[x][y][z].eject();
                }
            }
        }

        objList.Clear();
        StartCoroutine(LockFor(1));
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
        childObject.name = "Holder";

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

    // Проверяет тип объекта и либо размещает его сразу, либо помещает его в очередь
    protected void ProcessObject(GameObject obj) {

        var interactible = obj.GetComponent<Interactible>();

        // Это не объект для редактора
        if (!interactible) {
            return;
        }

        // Держит ли игрок объект
        var objIsActive = interactible.isActive;

        // Объект уже в редакторе
        if (objList.Contains(obj)) {

            if (objIsActive) {

                // Игрок взял объект, который ранее был поставлен в редактор
                RemoveObject(obj);
            }
            else {

                // Этот объект уже поставлен в редактор и его никто не трогает
                return;
            }
        }

        var rigidbody = obj.transform.parent.GetComponent<Rigidbody>();
        var objectIsMoving = !objIsActive && rigidbody && !rigidbody.velocity.Equals(Vector3.zero);

        // Размещаем объект сразу же, если игрок его отпустил и он движется
        if (objectIsMoving) {
            sinceLastUpdate = 0;

            DequeueObject(obj);
            PlaceObject(obj);
        } 
        else {
            // Иначе добавляем его в очередь
            EnqueueObject(obj);
        }

    }

    // Добавляет объект в очередь на добавление
    protected void EnqueueObject(GameObject obj) {
        if (!updateQueue.Contains(obj)) {
            updateQueue.Add(obj);
        }
    }

    // Удаляет и возвращает первый объект в очереди
    protected GameObject DequeueObject() {
        if (updateQueue.Count > 0) {
            var obj = updateQueue[0];
            updateQueue.RemoveAt(0);
            return obj;
        }
        return null;
    }

    // Удаляет объект из очереди
    protected void DequeueObject(GameObject obj) {
        if (updateQueue.Contains(obj)) {
            updateQueue.Remove(obj);
        }
    }

    // Размещает объект в редакторе или показывает браш, если есть место
    protected void PlaceObject(GameObject obj) {
        // Проверяем валидность объекта и находим ближайший блок, в который можно его поставить
        Block[] closestAffectedBlocks;
        var closestBlockCoord = GetClosestBlockCoord(obj, out closestAffectedBlocks);

        // Блок найден
        if (closestBlockCoord != -Vector3i.one) {

            HideBrush();

            // Считаем поворот объекта
            var appliedRotation = CalculateRotation(obj);
            var objIsActive = obj.GetComponent<Interactible>().isActive;

            // Если он в руке игрока, показываем кисть
            if (objIsActive) {
                ShowBrush(closestBlockCoord, obj, appliedRotation);
            }
            else {
                // Иначе ставим объект
                AddObject(closestBlockCoord, obj, closestAffectedBlocks, appliedRotation);
            }
        }
        else if (brushShownFor == obj) {
            // Прячем кисть, если для объекта нет места и кисть показана для него
            HideBrush();
        }
    }

    // Вовзращает координаты ближайшего валидного блока, а также блоки, на которые будет наложен GameObject и находится ли он в руке игрока
    // Возращает -1 вектор, если такого блока нет
    protected Vector3i GetClosestBlockCoord(GameObject obj, out Block[] closestAffectedBlocks) {
        closestAffectedBlocks = null;   

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

    // Пуст ли блок
    protected bool BlockIsEmpty(Block block, bool mustExist = false) {
        return !mustExist && block == null || block != null && (block.isEmpty || block.has(brush));
    }

    // Проверяет, не пересечется ли объект с другими заполненными блоками и собирает все пересеченные блоки в массив
    // Также проверяет прилегает ли блок к ранее поставленному объекту
    protected bool ObjectFits(Vector3i blockCoord, GameObject obj, out Block[] affectedBlocks) {
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

                    // Объект не удастся разместить, т.к. блок занят или не существует
                    if (!BlockIsEmpty(affectedBlock, true)) {
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
    protected bool BlockIsConnected(int x, int y, int z) {
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
    protected Quaternion CalculateRotation(GameObject obj) {
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
        return VectorUtils.Max(VectorUtils.RoundAroundToInt(VectorUtils.Divide(objSize, blockSize), maxObjectProtrusion), Vector3i.one);
    }

    // Возвращает размер объекта с округлением до ближайшего блока (минимум один блок)
    protected Vector3 CalculateMinFitSize(GameObject obj) {
        var objSize = CalculateSize(obj);
        return Vector3.Scale(VectorUtils.Max(VectorUtils.RoundAroundToInt(VectorUtils.Divide(objSize, blockSize), maxObjectProtrusion), Vector3i.one), blockSize);
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
