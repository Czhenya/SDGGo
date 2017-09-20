using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoomSelectManager : Singleton<RoomSelectManager> {

    public GameObject RoomPrefab;
    public Button[] roomButtons;
    List<string> roomList;
	// Use this for initialization
	void Start () {
        // 请求房间列表
        roomList = new List<string>();
        roomList.Add("111");
        roomList.Add("222");
        roomList.Add("333");
        roomList.Add("444");
        roomList.Add("555");
        for (int i= 0; i<roomList.Count;++i) {
            roomButtons[i].gameObject.SetActive(true);
            roomButtons[i].gameObject.GetComponentInChildren<Text>().text = roomList[i];
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(1)) {
            SceneManager.LoadScene("Menu");
        }
	}

    

    // 创建新房间
    public void CreateRoom() { 
    }

    public void EnterRoom(int index) {
        Debug.Log("enter room:"+roomList[index]);
    }
}
