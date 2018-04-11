using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {

    private static SceneLoader sceneLoader;
    

    public static SceneLoader instance {
        get {
            if (!sceneLoader) {
                sceneLoader = FindObjectOfType(typeof(SceneLoader)) as SceneLoader;

                if (!sceneLoader) {
                    Debug.LogError("There needs to be one active SceneLoader script on a GameObject in your scene.");
                }
                else {
                    sceneLoader.Init();
                }
            }

            return sceneLoader;
        }
    }

    void Init() {

    }

    public void LoadScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }
}
