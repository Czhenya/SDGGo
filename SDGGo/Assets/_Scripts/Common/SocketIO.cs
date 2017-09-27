using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json;
using UnityEngine.UI;
using SDG;

public class SocketIO : Singleton<SocketIO> {

    string serverURL = "http://10.246.60.27:8888";

    public Socket sdgSocket;
    public static bool isConnected = false; // 连接状态
    public string tiptext="";
    object locker = new object();

	void Start () {
        OpenSocket();
    }

    void OnDestroy()
    {
        CloseSocket();
    }

    void TIP(string txt) {
        lock (tiptext)
        {
            tiptext = txt;
        }
    }

    #region 对外接口
    // 初始化并开始socket监听
    public void OpenSocket()
    {
        TIP("trying to connect socket server...");
        if (sdgSocket == null)
        {
            sdgSocket = IO.Socket(serverURL);

            sdgSocket.On("connect", () => {
                TIP("socket connected!");
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

#endregion
}