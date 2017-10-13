using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void BackToLogin() {
        SceneManager.LoadScene("Wellcome");
    }

    public void GameHH() {
        SceneManager.LoadScene("GAME_H_H");
    }

    public void GameHC() {
        SceneManager.LoadScene("GAME_H_C");
    }

    public void OnlineGame() {
        SceneManager.LoadScene("RoomSelect");
    }

}
