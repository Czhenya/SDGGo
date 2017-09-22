using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using SDG;

public class RoomSelectManager : Singleton<RoomSelectManager> {

    public GameObject RoomPrefab = null;
    public Button[] roomButtons = null;

    List<RoomInfo> roomList = new List<RoomInfo>();   // 房间列表容器
    ParamBase param = new ParamBase();                // 参数对象
    bool request = false;                             // 请求房间列表状态
    int roomid = -1;                                  // 进入房间号
    object locker = new object();

	// Use this for initialization
	void Start () {
        // 请求房间列表
        //roomList.Add(new RoomInfo(111,"user1"));
        //roomList.Add(new RoomInfo(222, "user2"));

        param.userid = int.Parse(CurrentPlayer.Ins.user.userid);
        param.token = CurrentPlayer.Ins.user.token;

        // 监听房间列表响应
        SocketIO.Ins.sdgSocket.On("RetGetRoomList",(data)=> {
            Debug.Log("获取房间列表响应！");
            lock (locker) {
                Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString());
                string code = dic["code"].ToString();
                if (code == "0")
                {
                    object objroomlist = dic["roomList"];
                    List<RoomInfo> roomObjs = JsonConvert.DeserializeObject<List<RoomInfo>>(objroomlist.ToString());
                    roomList.AddRange(roomObjs);
                    request = true;
                }
            }
        });

        // 监听创建房间响应
        SocketIO.Ins.sdgSocket.On("RetCreateRoom", (data)=> {
            Debug.Log("创建房间响应！");
            lock (locker) {
                Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString());
                string code = dic["code"].ToString();
                if (code == "0")
                {
                    roomid = int.Parse(dic["roomid"].ToString());
                    CurrentPlayer.Ins.roomId = roomid;
                    CurrentPlayer.Ins.isRoomOwner = true;
                }
            }
        });

        // 请求房间列表
        GetRoomList();
        
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(1)) {
            SceneManager.LoadScene("Menu");
        }

        // 显示房间列表
        if (request) {
            request = false;
            for (int i = 0; i < roomList.Count; ++i)
            {
                if (i > 10) break;

                roomButtons[i].gameObject.SetActive(true);
                roomButtons[i].gameObject.GetComponentInChildren<Text>().text = "room" + roomList[i].roomid;
            }
        }

        // 进入房间
        if (roomid != -1) {
            EnterRoom();
        }
	}

    // 请求房间列表
    public void GetRoomList() {
        SocketIO.Ins.OpenSocket();
        // 请求房间列表
        if (SocketIO.isConnected)
        {
            string paramstr = JsonConvert.SerializeObject(param);
            SocketIO.Ins.sdgSocket.Emit("ReqGetRoomList", paramstr);
        }
    }

    // 创建新房间
    public void CreateRoom() {
        SocketIO.Ins.OpenSocket();
        if (SocketIO.isConnected)
        {
            string paramstr = JsonConvert.SerializeObject(param);
            SocketIO.Ins.sdgSocket.Emit("ReqCreateRoom", paramstr);
        }
    }

    // 选择进入房间
    public void EnterRoom(int index) {
        RoomInfo info = roomList[index];
        roomid = info.roomid;
        EnterRoom();
    }
    void EnterRoom() {
        Debug.Log("enter room:" + roomid);
        CurrentPlayer.Ins.roomId = roomid;
        SceneManager.LoadScene("GAME_ROOM");
    }
}