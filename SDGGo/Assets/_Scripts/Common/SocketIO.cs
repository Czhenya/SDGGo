using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json;
using SDG;

public class SocketIO : Singleton<SocketIO> {

    string serverURL = "http://10.246.60.27:8000";

    public Socket sdgSocket;
    public static bool isConnected = false;

	// Use this for initialization
	void Start () {
        OpenSocket();
	}

    void OnDestroy()
    {
        CloseSocket();
    }


    #region 对外接口
    // 初始化并开始socket监听
    public void OpenSocket()
    {
        if (sdgSocket == null)
        {
            sdgSocket = IO.Socket(serverURL);

            sdgSocket.On(Socket.EVENT_CONNECT, () => {
                Debug.Log("socket connected!");
                isConnected = true;
            });
        }
    }

    // 断开socket监听
    void CloseSocket()
    {
        if (sdgSocket != null)
        {
            sdgSocket.Disconnect();
            sdgSocket = null;
            isConnected = false;
        }
        Debug.Log("socket closed!");
    }
    // 登录
    public void Login(ParamBase param) {
        string paramstr = JsonConvert.SerializeObject(param);
        
        if (sdgSocket != null) {
            Debug.Log("登录请求参数：" + paramstr);
            sdgSocket.Emit("ReqSignIn",paramstr);
        }
    }

    public void Signup(ParamBase param)
    {
        string paramstr = JsonConvert.SerializeObject(param);
        
        if (sdgSocket != null)
        {
            Debug.Log("注册请求参数：" + paramstr);
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