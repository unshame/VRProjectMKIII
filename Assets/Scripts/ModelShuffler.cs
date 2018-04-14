using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class ModelShuffler : MonoBehaviour {

    [System.Serializable]
    public class ModelArray {
        public Mesh mesh;
        public Material[] materials;
    }

    public ModelArray[] models;
    public List<int> specifiedInput = new List<int>(){ 0 };

    public void ShuffleOnSpecifiedInput(int i) {
        if(specifiedInput.Contains(i)) {
            ShuffleModel();
        }
    }

    public void ShuffleModel() {
        var model = models[Random.Range(0, models.Length)];
        var mesh = model.mesh;
        var material = model.materials[Random.Range(0, model.materials.Length)];
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;
    }
}
