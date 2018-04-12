//#define GRID_DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Conditional = System.Diagnostics.ConditionalAttribute;

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

    // Свободные объекты внутри поля, которые уже были обновлены
    protected List<GameObject> movingObjects = new List<GameObject>();

    // Массив для подходящих для размещения объекта блоков
    protected List<KeyValuePair<float, Vector3i>> possibleBlocks = new List<KeyValuePair<float, Vector3i>>();


    /* События Unity */

    // Считает размер блоков
    protected virtual void Awake() {
        OpenDebugFile();
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
                        CreateBlockAnchor(),
                        new Vector3i(size.x - x - 1, size.y - y - 1, size.z - z - 1)
                    );
                }
            }
        }
    }

    protected virtual void OnDestroy() {
        CloseDebugFile();
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
        var obj = other.gameObject;

        DequeueObject(obj);

        if (movingObjects.Contains(obj)) {
            movingObjects.Remove(obj);
        }

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
        block.fill(brush, false, offset, rotation);
        brushBlock = block;
        brushShownFor = obj;

        // Устанавливаем меш и масштаб кисти
        brush.GetComponent<MeshFilter>().mesh = obj.GetComponent<MeshFilter>().mesh;
        brush.transform.localScale = obj.transform.localScale;
    }

    // Убирает указанный GameObject из редактора
    public virtual void RemoveObject(GameObject obj) {

        if ((obj != brush || brushBlock == null) && !objList.Contains(obj)) return;

        RemoveObjectFromBlocks(obj);

        objList.Remove(obj);

        UpdateBlockSpaces();
    }

    private void RemoveObjectFromBlocks(GameObject obj) {
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                for (int z = 0; z < size.z; z++) {
                    var block = blocks[x][y][z];
                    if (block.gameObject == obj) {
                        if (block.objBlockReach != -Vector3i.one) {
                            SetBlocksFilled(block, block.objBlockReach, false);
                        }
                        block.empty(obj);
                        return;
                    }
                }
            }
        }
    }

    // Добавляет GameObject по указанным координатам
    public virtual void AddObject(Vector3i blockCoord, GameObject obj, Quaternion rotation) {

        var block = GetBlock(blockCoord);
        var offset = CalculateOffset(obj);
        var objBlockReach = CalculateBlockMagnitude(obj) - Vector3i.one;

        // Заполняем массив объектов и блоки объектом
        objList.Add(obj);
        block.fill(obj, true, offset, rotation, objBlockReach);
        SetBlocksFilled(block, objBlockReach, true);

        WriteDebug(obj);

        if (movingObjects.Contains(obj)) {
            movingObjects.Remove(obj);
        }

        UpdateBlockSpaces();
    }

    // Отключает редактирование
    public virtual void Lock() {
        for (int i = 0; i < objList.Count; i++) {
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

        UpdateBlockSpaces();

        objList.Clear();
        StartCoroutine(LockFor(1));
    }


    /* Работа с блоками */

    // Возвращает блок по координатам
    public Block GetBlock(int x, int y, int z) {
        if (x < 0 || x >= size.x || y < 0 || y >= size.y || z < 0 || z >= size.z) {
            return null;
        }

        return blocks[x][y][z];
    }

    public Block GetBlock(Vector3i coord) {
        return GetBlock(coord.x, coord.y, coord.z);
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

    // Обновляет счетчики пустого места после блоков
    protected void UpdateBlockSpaces() {
        WriteToFileDebug("================================");

        for (int x = size.x - 1; x >= 0; x--) {

            WriteToFileDebug();
            WriteToFileDebug();
            WriteToFileDebug(string.Format("({0})", x).PadRight(4));
            for (int y = size.y - 1; y >= 0; y--) {
                WriteToFileDebug(y.ToString().PadLeft(15));
            }

            for (int y = size.y - 1; y >= 0; y--) {

                WriteToFileDebug();
                WriteToFileDebug(string.Format("{0}: ", y).PadLeft(5));

                for (int z = size.z - 1; z >= 0; z--) {
                    var block = blocks[x][y][z];

                    // Блок не пуст, заполняем минус единицами
                    if (BlockIsEmpty(block)) {
                        // Блоки после этого
                        var blockx = GetBlock(x + 1, y, z);
                        var blocky = GetBlock(x, y + 1, z);
                        var blockz = GetBlock(x, y, z + 1);

                        // Если блоков не существует, то места нет
                        var sx = blockx == null ? 0 : blockx.spaces.x + 1;
                        var sy = blocky == null ? 0 : blocky.spaces.y + 1;
                        var sz = blockz == null ? 0 : blockz.spaces.z + 1;
                        block.spaces = new Vector3i(sx, sy, sz);

                        if (BlockIsEmpty(blockx) && BlockIsEmpty(blocky) && BlockIsEmpty(blockz)) {
                            var cx = blockx == null || blockx.connectedAfter.x == -1 ? -1 : blockx.connectedAfter.x + 1;
                            var cy = blocky == null || blocky.connectedAfter.y == -1 ? -1 : blocky.connectedAfter.y + 1;
                            var cz = blockz == null || blockz.connectedAfter.z == -1 ? -1 : blockz.connectedAfter.z + 1;
                            block.connectedAfter = new Vector3i(cx, cy, cz);
                        }
                        else {
                            block.connectedAfter = Vector3i.zero;
                        }

                    }
                    else {
                        block.spaces = -Vector3i.one;
                        block.connectedAfter = Vector3i.zero;
                    }

                    var xx = size.x - 1 - x;
                    var yy = size.y - 1 - y;
                    var zz = size.z - 1 - z;
                    var otherBlock = blocks[xx][yy][zz];
                    var blockxx = GetBlock(xx - 1, yy, zz);
                    var blockyy = GetBlock(xx, yy - 1, zz);
                    var blockzz = GetBlock(xx, yy, zz - 1);
                    if (BlockIsEmpty(otherBlock) && BlockIsEmpty(blockxx) && BlockIsEmpty(blockyy) && BlockIsEmpty(blockzz)) {
                        var cxx = blockxx == null || blockxx.connectedBefore.x == -1 ? -1 : blockxx.connectedBefore.x + 1;
                        var cyy = blockyy == null || blockyy.connectedBefore.y == -1 ? -1 : blockyy.connectedBefore.y + 1;
                        var czz = blockzz == null || blockzz.connectedBefore.z == -1 ? -1 : blockzz.connectedBefore.z + 1;
                        otherBlock.connectedBefore = new Vector3i(cxx, cyy, czz);
                    }
                    else {
                        otherBlock.connectedBefore = Vector3i.zero;
                    }

                    //WriteToFileDebug(string.Format("{0} ", block.spaces).PadLeft(15));
                    //WriteToFileDebug(string.Format("{0} ", block.connectedAfter).PadLeft(15));
                    WriteToFileDebug(string.Format("{1}{0} ", otherBlock.connectedBefore, BlockIsEmpty(otherBlock) ? "" : "*").PadLeft(15));
                }
            }
        }
        WriteToFileDebug();
    }

    protected void SetBlocksFilled(Block sourceBlock, Vector3i objBlockReach, bool value) {
        var rangeStart = sourceBlock.coord;
        var rangeEnd = sourceBlock.coord + objBlockReach;
        for (int x = rangeStart.x; x <= rangeEnd.x; x++) {
            for (int y = rangeStart.y; y <= rangeEnd.y; y++) {
                for (int z = rangeStart.z; z <= rangeEnd.z; z++) {
                    var block = GetBlock(x, y, z);
                    if (block == sourceBlock) continue;
                    if(value) {
                        block.fill(sourceBlock);
                    }
                    else {
                        block.empty();
                    }
                }
            }
        }
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

        var rigidbody = obj.transform.GetComponent<Rigidbody>();
        var objectIsMoving = !objIsActive && rigidbody && !rigidbody.velocity.Equals(Vector3.zero);

        // Размещаем объект сразу же, если игрок его отпустил, он движется и мы его раньше не обрабатывали
        if (objectIsMoving && !movingObjects.Contains(obj)) {
            movingObjects.Add(obj);
            DequeueObject(obj);
            PlaceObject(obj);
            return;
        }

        // Объект снова в руке игрока, убираем его из списка свободных
        if (objIsActive && movingObjects.Contains(obj)) {
            movingObjects.Remove(obj);
        }

        // Добавляем объект в очередь
        EnqueueObject(obj);
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
        var closestBlockCoord = GetClosestBlockCoord(obj);

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
                AddObject(closestBlockCoord, obj, appliedRotation);
            }
        }
        else if (brushShownFor == obj) {
            // Прячем кисть, если для объекта нет места и кисть показана для него
            HideBrush();
        }
    }

    // Возвращает координаты ближайшего валидного блока, а также блоки, на которые будет наложен GameObject и находится ли он в руке игрока
    // Возвращает -1 вектор, если такого блока нет
    protected Vector3i GetClosestBlockCoord(GameObject obj) {

        // Рассчитываем позицию объекта и диапазон блоков
        Vector3i rangeStart, rangeEnd;
        var objPosition = CalculatePositionAndRanges(obj, out rangeStart, out rangeEnd);

        // Находим все пустые блоки в диапазоне, в которые уместится объект
        possibleBlocks.Clear();
        for (int x = rangeStart.x; x <= rangeEnd.x; x++) {
            for (int y = rangeStart.y; y <= rangeEnd.y; y++) {
                for (int z = rangeStart.z; z <= rangeEnd.z; z++) {

                    // Проверяем валидность блока, находим растояние до блока и задетые блоки
                    var blockCoord = new Vector3i(x, y, z);
                    var block = GetBlock(blockCoord);
                    if (block == null) continue;

                    var dist = Vector3.Distance(objPosition, block.position); ;

                    if (BlockIsEmpty(block) && ObjectFits(blockCoord, obj)) {
                        possibleBlocks.Add(new KeyValuePair<float, Vector3i>(dist, blockCoord));
                    }
                }
            }
        }

        // Сортируем найденные блоки по дальности от объекта
        possibleBlocks.Sort((x1, x2) => {
            var dist1 = x1.Key;
            var dist2 = x2.Key;
            if (dist1 == dist2 || Mathf.Abs(dist1 - dist2) < Mathf.Epsilon) return 0;
            if (dist1 - dist2 > 0) return 1;
            return -1;
        });

        // Ищем блок, в котором объект будет соединен с уже поставленным объектом и находим все задетые блоки
        for (int i = 0; i < possibleBlocks.Count; i++) {
            var blockCoord = possibleBlocks[i].Value;
            if (ObjectIsConnected(blockCoord, obj)) {
                // Debug.LogFormat("{0} {1}", CalculateBlockMagnitude(obj), GetBlock(blockCoord).spaces);
                return blockCoord;
            }
        }

        return -Vector3i.one;
    }

    // Пуст ли блок
    protected bool BlockIsEmpty(Block block, bool mustExist = false) {
        return !mustExist && block == null || block != null && (block.isEmpty || block.has(brush));
    }

    // Проверяет, не пересечется ли объект с другими заполненными блоками
    protected bool ObjectFits(Vector3i blockCoord, GameObject obj) {
        var blockNumAfter = CalculateBlockMagnitude(obj) - Vector3i.one;

        var dec = new Vector3i(1, 0, 1);
        while (blockNumAfter.x != -1 && blockNumAfter.z != -1) {
            var block = GetBlock(blockCoord);

            // Проверяем вмещается ли объект по оси y
            if (blockNumAfter.y > block.spaces.y) return false;

            // Двигаем позицию объекта снизу вверх и проверяем вмещение по осям x и z
            var reachY = blockCoord.y + blockNumAfter.y;
            for (int y = blockCoord.y; y <= reachY; y++) {
                var otherBlock = GetBlock(blockCoord.x, y, blockCoord.z);
                if (blockNumAfter.x > otherBlock.spaces.x || blockNumAfter.z > otherBlock.spaces.z) return false;
            }

            // Сдвигаем координаты диагонально по горизонтали 
            blockNumAfter -= dec;
            blockCoord += dec;
        }

        return true;
    }

    // Проверяет соединен ли объект с другими блоками в данной позиции и находит задетые блоки
    protected bool ObjectIsConnected(Vector3i blockCoord, GameObject obj) {

        if (blockCoord.y == 0) return true;

        var objBlockMagnitude = CalculateBlockMagnitude(obj);

        // Самый дальний блок, который будет занят объектом
        var objBlockReach = blockCoord + objBlockMagnitude - Vector3i.one;

        var x1 = blockCoord.x;
        var y1 = blockCoord.y;
        var z1 = blockCoord.z;

        var x2 = objBlockReach.x;
        var y2 = objBlockReach.y;
        var z2 = objBlockReach.z;

        for (int x = x1; x <= x2; x++) {
            var conn1 = GetBlock(x, y1, z1).connectedAfter;
            var conn2 = GetBlock(x, y2, z2).connectedBefore;

            if (
                conn1.y != -1 && conn1.y < objBlockMagnitude.y || 
                conn1.z != -1 && conn1.z < objBlockMagnitude.z || 
                conn2.y != -1 && conn2.y < objBlockMagnitude.y ||
                conn2.z != -1 && conn2.z < objBlockMagnitude.z
            ) {
                return true;
            }
        }

        for (int z = z1 + 1; z <= z2; z++) {
            var conn1 = GetBlock(x1, y1, z).connectedAfter;
            var conn2 = GetBlock(x2, y2, z).connectedBefore;

            if (
                conn1.y != -1 && conn1.y < objBlockMagnitude.y ||
                conn2.y != -1 && conn2.y < objBlockMagnitude.y
            ) {
                return true;
            }
        }

        return false;
    }

    // Проверяет, находится ли блок рядом с хотя бы одним уже заполненным
    protected bool BlockIsConnected(int x, int y, int z) {
        if (y == 0) return true;

        var coord = new Vector3i(x, y, z);

        // Проверяем пустоту ближаших блоков по каждой оси
        for (int i = 0; i < 3; i++) {
            var increment = Vector3i.zero;
            increment[i] = 1;

            if (!BlockIsEmpty(GetBlock(coord + increment)) || !BlockIsEmpty(GetBlock(coord - increment))) {
                return true;
            }
        }

        return false;
    }


    /* Размеры и позиции объекта */

    // Считает поворот объекта
    // Учитывается только поворот в identity блока, реальный поворот применяется позже только визуально
    protected Quaternion CalculateRotation(GameObject obj) {
        Quaternion appliedRotation = Quaternion.identity;
        var identity = obj.GetComponent<ObjectIdentity>();
        return identity ? identity.GetRotation() : Quaternion.identity;
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

    // Считает позицию объекта с оффсетом и диапазон подходящих координат блоков
    protected Vector3 CalculatePositionAndRanges(GameObject obj, out Vector3i rangeStart, out Vector3i rangeEnd) {

        // Реальная позиция с учетом оффсета
        var offset = CalculateOffset(obj);
        var objPosition = obj.transform.position - transform.rotation * offset;

        // Нам нужна локальная позиция повернутая в противоположную сторону редактору
        // Вращаем вокруг центра редактора
        var localObjPosition = (Quaternion.Inverse(transform.rotation) * (obj.transform.position - transform.position) + transform.localScale / 2 - offset);

        // Локальная позиция в блоках
        var localObjPositionInBlocks = Vector3.Scale(localObjPosition, VectorUtils.Divide(size, transform.localScale));

        // Сколько блоков вокруг текущего нам подходят
        var addedRange = new Vector3(maxBlockDistance, maxBlockDistance, maxBlockDistance);

        // Начало и конец диапазона подходящих блоков
        rangeStart = VectorUtils.Max(VectorUtils.FloorToInt(localObjPositionInBlocks - addedRange), Vector3i.zero);
        rangeEnd = VectorUtils.Min(VectorUtils.CeilToInt(localObjPositionInBlocks + addedRange), size - Vector3i.one);

        // Debug.DrawRay(localObjPosition + transform.position - transform.localScale / 2, transform.localScale / 2);
        // Debug.LogFormat("{2} - {0} {1}", rangeStart, rangeEnd, localObjPosition);

        return objPosition;
    }


    /* Дебаг */

    public bool debugGridEnabled = false;

    public GameObject DebugGridPrefab;

    public string debugFilePath;
    StreamWriter debugFile;

    [Conditional("GRID_DEBUG")]
    protected void WriteDebug(GameObject obj) {
        if (!debugGridEnabled) return;
        var s1 = CalculateBlockMagnitude(obj);
        var s2 = CalculateSize(obj);
        var s3 = CalculateMinFitSize(obj);
        var s4 = CalculateRotation(obj);
        var s5 = GetObjectCoord(obj);
        Debug.LogFormat("{0} | {1} | {2} | {3} | {4}", s1, s2, s3, s4, s5);
    }

    [Conditional("GRID_DEBUG")]
    protected void OpenDebugFile() {
        if (debugFilePath != null && debugFilePath != "") {
            debugFile = new StreamWriter(debugFilePath, false);
        }
    }

    [Conditional("GRID_DEBUG")]
    protected void CloseDebugFile() {
        if (debugFile != null) {
            debugFile.Close();
        }
    }

    [Conditional("GRID_DEBUG")]
    protected void WriteToFileDebug(string str = "", bool newLine = false) {
        if (debugFile == null) return;
        if (newLine) {
            debugFile.WriteLine(str);
        }
        else {
            debugFile.Write(str);
        }
    }

    [Conditional("GRID_DEBUG")]
    protected void WriteToFileDebug() {
        if (debugFile != null) {
            debugFile.WriteLine();
        }
    }

}
