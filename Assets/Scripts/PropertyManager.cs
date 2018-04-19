using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Менеджер поворота блоков (синглтон)
// Хранит последний поворот различных типов блоков
public class PropertyManager : MonoBehaviour {

    private Dictionary<string, int> rotationDictionary;
    private Dictionary<string, int> sizeDictionary;

    private static PropertyManager propertyManager;

    public static BuildStation MainBuildStation {
        get {
            return instance.mainBuildStation;
        }
    }

    private BuildStation mainBuildStation;

    public static PropertyManager instance {
        get {
            if (!propertyManager) {
                propertyManager = FindObjectOfType(typeof(PropertyManager)) as PropertyManager;

                if (!propertyManager) {
                    Debug.LogError("There needs to be one active PropertyManager script on a GameObject in your scene.");
                }
                else {
                    propertyManager.Init();
                }
            }

            return propertyManager;
        }
    }

    void Init() {
        if (rotationDictionary == null) {
            rotationDictionary = new Dictionary<string, int>();
        }
        if (sizeDictionary == null) {
            sizeDictionary = new Dictionary<string, int>();
        }
        mainBuildStation = FindObjectOfType<DualBuildStation>();
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

    public static void SetSize(string typeName, int sizeIndex) {
        if (instance.sizeDictionary.ContainsKey(typeName)) {
            instance.sizeDictionary[typeName] = sizeIndex;
        }
        else {
            instance.sizeDictionary.Add(typeName, sizeIndex);
        }
    }

    public static int GetSize(string typeName) {
        if (instance.sizeDictionary.ContainsKey(typeName)) {
            return instance.sizeDictionary[typeName];
        }
        return -1;
    }
}