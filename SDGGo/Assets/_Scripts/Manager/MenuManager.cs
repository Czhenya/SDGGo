using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

    public Text text_account;
	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        text_account.text = CurrentPlayer.Ins.user.username + "[" + CurrentPlayer.Ins.user.userid + "]\n" + CurrentPlayer.Ins.opponent.username + "[" + CurrentPlayer.Ins.opponent.userid + "]";
    }

    public void BackToLogin() {
        // 断开连接
        SocketIO.Ins.CloseSocket();
        GoSceneManager.Ins.EnterLoadingScene("Wellcome");
    }

    public void GameHH() {
        GoSceneManager.Ins.EnterLoadingScene("GAME_H_H");
    }

    public void GameHC() {
        GoSceneManager.Ins.EnterLoadingScene("GAME_H_C");
    }

    public void OnlineGame() {
        GoSceneManager.Ins.EnterLoadingScene("RoomSelect");
    }

    public void EnterSetting() {
        GoSceneManager.Ins.EnterLoadingScene("Setting");
    }

}
