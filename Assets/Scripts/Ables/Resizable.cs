using UnityEngine;

class Resizable : MonoBehaviour {

    public Vector3[] sizes = new Vector3[0];
    public int currentSizeIndex = 0;

    private void Awake() {
        WrapSizeIndex();
        ApplySizeIndex();
    }

    public void NextSize(int step = 1) {
        currentSizeIndex += step;
        WrapSizeIndex();
        ApplySizeIndex();
        transform.localPosition = Vector3.zero;
    }

    public void PrevSize(int step = 1) {
        currentSizeIndex -= step;
        WrapSizeIndex();
        ApplySizeIndex();
        transform.localPosition = Vector3.zero;
    }

    private void ApplySizeIndex() {
        transform.localScale = sizes[currentSizeIndex];
    }

    private void WrapSizeIndex() {
        if (currentSizeIndex >= sizes.Length) {
            currentSizeIndex = currentSizeIndex % sizes.Length;
        }
        else if (currentSizeIndex < 0) {
            currentSizeIndex = sizes.Length - 1 - (Mathf.Abs(currentSizeIndex + 1) % sizes.Length);
        }
    }

}
