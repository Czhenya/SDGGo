using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json;
using SDG;

public class SocketIO : Singleton<SocketIO> {

    string serverURL = "http://139.196.193.91:8000";

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
        TIP("正在连接到服务器...");
        if (sdgSocket == null)
        {
            sdgSocket = IO.Socket(serverURL);

            sdgSocket.On("connect", () => {
                TIP("连接服务器成功!");
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
        Debug.Log("服务器断开!");
    }

#endregion
}