using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using SDG;

public class RoomSelectManager : Singleton<RoomSelectManager> {

    public GameObject RoomItemPrefab = null;
    public List<GameObject> roomItems = null;
    public GameObject RoomContent = null;

    List<RoomInfo> roomList = new List<RoomInfo>();   // 房间列表容器
    ParamBase param = new ParamBase();                // 参数对象
    bool request = false;                             // 请求房间列表状态
    bool isEnterRoom=false;                           // 进入房间
    object locker = new object();

	void Start () {
        RoomInfo room1 = new RoomInfo(111,"x小王");
        RoomInfo room2 = new RoomInfo(222,"albert");
        roomList.Add(room1);
        roomList.Add(room2);
        request = true;

        // 请求参数
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
                    roomList.Clear();
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
                    CurrentPlayer.Ins.roomId = dic["roomid"].ToString();
                    CurrentPlayer.Ins.isRoomOwner = true;
                    isEnterRoom = true;
                }
            }
        });

        // 监听进入房间
        SocketIO.Ins.sdgSocket.On("RetJoinRoom", (data)=> {
            Debug.Log("成功进入房间！");
            lock (locker) {
                Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString());
                string code = dic["code"].ToString();
                if (code == "0")
                {
                    CurrentPlayer.Ins.roomId = dic["roomid"].ToString();
                    CurrentPlayer.Ins.isRoomOwner = false;
                    CurrentPlayer.Ins.opponent.userid = dic["userid"].ToString();
                    CurrentPlayer.Ins.opponent.username = dic["name"].ToString();
                    isEnterRoom = true;
                }
            }
        });

        // 请求房间列表
        GetRoomList();
    }
	
	void Update () {
        // 刷新房间列表
        if (request) {
            request = false;
            UpdateRoomList();
        }

        // 进入房间
        if (isEnterRoom) {
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

    // 刷新房间列表
    void UpdateRoomList() {
        // 清空
        for (int i = 0; i < RoomContent.transform.childCount; i++)
        {
            Destroy(RoomContent.transform.GetChild(i).gameObject);
         }
        // 刷新
        for (int i = 0; i < roomList.Count; ++i)
        {
            // 新房间项坐标
            float item_margin = UIRoomItem.margin;
            float item_height = UIRoomItem.height;
            Vector3 position = new Vector3(0, -(item_margin * (i + 1) + item_height * i));
            GameObject room_item = Instantiate(RoomItemPrefab);
            room_item.transform.parent = RoomContent.transform;
            room_item.transform.localPosition = position;
            room_item.transform.localScale = new Vector3(1,1,1);
            roomItems.Add(room_item);

            UIRoomItem uirromitem = roomItems[i].GetComponent<UIRoomItem>();
            uirromitem.room_index = i;
            uirromitem.room_name.text = "房间号：" + roomList[i].roomid;
            uirromitem.room_info.text = "房主：" + roomList[i].owner;
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
        ParamRoom param_room = new ParamRoom();
        param_room.userid = param.userid;
        param_room.token = param.token;
        param_room.roomid = info.roomid;
        string paramstr = JsonConvert.SerializeObject(param_room);
        SocketIO.Ins.sdgSocket.Emit("ReqJoinRoom", paramstr);
    }
    void EnterRoom() {
        Debug.Log("enter room:" + CurrentPlayer.Ins.roomId);
        SceneManager.LoadScene("GAME_ROOM");
    }

    // 返回主菜单
    public void BackHome() {
        SceneManager.LoadScene("Menu");
    }
}