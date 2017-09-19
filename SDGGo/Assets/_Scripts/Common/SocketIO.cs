using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json;
using SDG;

public class SocketIO : Singleton<SocketIO> {

    string serverURL = "http://10.246.60.27:8000";

    Socket sdgSocket;

	// Use this for initialization
	void Start () {
        OpenSocket();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // 初始化并开始socket监听
    void OpenSocket() {
        if (sdgSocket == null) {
            sdgSocket = IO.Socket(serverURL);

            sdgSocket.On(Socket.EVENT_CONNECT,()=> {
                Debug.Log("socket connected!");
            });

            sdgSocket.On("ReqSignUp", (data) => {
                object obj = data;
            });

            sdgSocket.On("ReqSignIn", (data) => {
                object obj = data;
            });
        }
    }

    // 断开socket监听
    void CloseSocket() {
        if (sdgSocket != null) {
            sdgSocket.Disconnect();
            sdgSocket = null;
        }
    }

#region 对外接口
    // 对外接口
    public void Login(ParamLogin param) {
        string paramstr = JsonConvert.SerializeObject(param);
        Debug.Log(paramstr);
        if (sdgSocket != null) {
            sdgSocket.Emit("ReqSignIn",paramstr);
        }
    }

    public void Signup(ParamLogin param)
    {
        string paramstr = JsonConvert.SerializeObject(param);
        if (sdgSocket != null)
        {
            sdgSocket.Emit("ReqSignUp", paramstr);
        }
    }

    public void GetRoomList() {
    }

    public void CreateRoom() {
    }

    public void JoinRoom() {
    }

    public void PlayMove(ParamPlayMove param) {
        string paramstr = JsonConvert.SerializeObject(param);
        if (sdgSocket != null) {
            sdgSocket.Emit("",paramstr);
        }
    }

    public void GameStart() {
    }

    public void GameOver() {
    }

#endregion
}