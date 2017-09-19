using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomButton : MonoBehaviour {

    public int roomNum;
	// Use this for initialization
	void Start () {
        gameObject.GetComponent<Button>().onClick.AddListener(() => {
            RoomSelectManager.Ins.EnterRoom(roomNum);
        });
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
