using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoSceneManager : Singleton<GoSceneManager> {

    public static string scene = null;

    // 异步加载场景
    // 过渡场景
    public void EnterLoadingScene(string sceneName) {
        scene = sceneName;
        SceneManager.LoadScene("Progress");
    } 
}
