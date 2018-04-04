using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationManager : MonoBehaviour {

    public Dictionary<string, int> rotationDictionary;

    private static RotationManager rotationManager;

    public static RotationManager instance {
        get {
            if (!rotationManager) {
                rotationManager = FindObjectOfType(typeof(RotationManager)) as RotationManager;

                if (!rotationManager) {
                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
                else {
                    rotationManager.Init();
                }
            }

            return rotationManager;
        }
    }

    void Init() {
        if (rotationDictionary == null) {
            rotationDictionary = new Dictionary<string, int>();
        }
    }

    public static void SetRotation(string typeName, int rotationIndex) {
        if (instance.rotationDictionary.ContainsKey(typeName)) {
            instance.rotationDictionary[typeName] = rotationIndex;
        }
        else {
            instance.rotationDictionary.Add(typeName, rotationIndex);
        }
    }

    public static int GetRotation(string typeName) {
        if (instance.rotationDictionary.ContainsKey(typeName)) {
            return instance.rotationDictionary[typeName];
        }
        return -1;
    }
}