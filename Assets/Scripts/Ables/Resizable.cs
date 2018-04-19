using UnityEngine;

// Компонент изменения размера объекта на указанные размеры
// Хранит, изменяет, сохраняет и загружает индекс размера и устанавливает размер
class Resizable : MonoBehaviour {

    public Vector3[] sizes = new Vector3[0];
    [SerializeField]
    private int currentSizeIndex = 0;

    void Awake() {
        LoadSizeIndex();
        ApplySizeIndex(false);
    }

    public void NextSize(int step = 1) {
        currentSizeIndex = Mathf.Clamp(currentSizeIndex + step, 0, sizes.Length - 1);
        SaveSizeIndex();
        ApplySizeIndex();        
    }

    public void PrevSize(int step = 1) {
        currentSizeIndex = Mathf.Clamp(currentSizeIndex - step, 0, sizes.Length - 1);
        SaveSizeIndex();
        ApplySizeIndex();
    }

    public int GetSizeIndex() {
        return currentSizeIndex;
    }

    public void SetSizeIndex(int sizeIndex) {
        currentSizeIndex = sizeIndex;
        WrapSizeIndex();
    }

    public void ApplySizeIndex(bool resetLocalPosition = true) {
        transform.localScale = sizes[currentSizeIndex];
        if (resetLocalPosition) {
            transform.localPosition = Vector3.zero;
        }
    }

    public void LoadSizeIndex() {
        var typeName = GetComponent<ObjectIdentity>().typeName;
        var sizeIndex = PropertyManager.GetSize(typeName);
        if (sizeIndex != -1) {
            currentSizeIndex = sizeIndex;
        }
    }

    public void SaveSizeIndex() {
        var typeName = GetComponent<ObjectIdentity>().typeName;
        PropertyManager.SetSize(typeName, currentSizeIndex);
    }

    void WrapSizeIndex() {
        if (currentSizeIndex >= sizes.Length) {
            currentSizeIndex = currentSizeIndex % sizes.Length;
        }
        else if (currentSizeIndex < 0) {
            currentSizeIndex = sizes.Length - 1 - (Mathf.Abs(currentSizeIndex + 1) % sizes.Length);
        }
    }

}
