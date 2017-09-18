using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour {

    public int roomNum;
	// Use this for initialization
	void Start () {
        gameObject.GetComponent<Button>().onClick.AddListener(() => {
            EnterRoom(roomNum);
        });
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // 进入房间
    public void EnterRoom(int roomN)
    {
        Debug.Log("room:" + roomN);
    }
}
