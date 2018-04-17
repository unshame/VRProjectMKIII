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

    [System.Serializable]
    private struct _ModelArray {
        public Mesh mesh;
        public Material material;
    }

    public ModelArray[] models;
    private List<_ModelArray> _models = new List<_ModelArray>();
    public List<int> specifiedInput = new List<int>(){ 0 };

    public void Start() {
        for(int i = 0; i < models.Length; i++) {
            for(int j = 0; j < models[i].materials.Length; j++) {
                _models.Add(new _ModelArray {
                    mesh = models[i].mesh,
                    material = models[i].materials[j]
                });
            }
        }
    }

    public void ShuffleOnSpecifiedInput(int i) {
        if(specifiedInput.Contains(i)) {
            ShuffleModel();
        }
    }

    public void ShuffleModel() {
        var model = _models[Random.Range(0, _models.Count)];
        GetComponent<MeshFilter>().mesh = model.mesh;
        GetComponent<MeshRenderer>().material = model.material;
    }
}
