using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using SDG;
public class WellComeManager : MonoBehaviour
{

    public InputField username = null;
    public InputField password = null;
    public Text tip = null;

    int curStat = 0;   // 0: 无操作 1：登录成功 2：登录失败 3： 注册成功 4：注册失败
    static object locker = new object();

    // Use this for initialization
    void Start()
    {
        // 监听登录响应
        SocketIO.Ins.sdgSocket.On("RetSignIn", (data) => {
        Debug.Log("登录返回正常！");
            Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString());
            string code = dic["code"].ToString();
            
             if (code == "0")
             {
                Debug.Log("登录成功！");
                lock (locker) {
                 curStat = 1;
                 CurrentPlayer.Ins.user.userid = dic["userid"].ToString();
                 CurrentPlayer.Ins.user.token = dic["token"].ToString();
                 }
             }
             else {
                Debug.Log("登录失败！");
                lock (locker) {
                     curStat = 2;
                 }
             }
             
        });

        // 监听注册响应
        SocketIO.Ins.sdgSocket.On("RetSignUp", (data) => {
            Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString());
            string code = dic["code"].ToString();
            if (code == "0")
            {
                lock (locker) {
                    Debug.Log("注册成功！");
                    curStat = 3;
                }
            }
            else {
                lock (locker) {
                    Debug.Log("注册失败！");
                    curStat = 4;
                }
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        // 主线程更新UI
        switch (curStat) {
            case 0: Tip("");break;
            case 1: Tip("登录成功！");LoginSuccess(); break;
            case 2: Tip("登录失败！"); break;
            case 3: Tip("注册成功！"); break;
            case 4: Tip("注册失败！"); break;
            default:Tip(""); break;
        }
    }

    void LoginSuccess() {
        SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// 提示
    /// </summary>
    /// <param name=""></param>
    void Tip(string msg) {
        tip.text = msg;
    }

    /// <summary>
    ///  登录
    /// </summary>
    public void Login()
    {
        string uname = username.text;
        string upwd = password.text;
        CurrentPlayer.Ins.user.username = uname;
        CurrentPlayer.Ins.user.password = upwd;
        ParamBase param = new ParamBase();
        param.name = uname;
        if (SocketIO.isConnected)
        {
            SocketIO.Ins.Login(param);
        }
        else {
            Tip("网络尚未连接，请稍后再试！");
        }
    }

    /// <summary>
    /// 注册
    /// </summary>
    public void SignUp() {
        string uname = username.text;
        string upwd = password.text;
        ParamBase param = new ParamBase();
        param.name = uname;
        if (SocketIO.isConnected)
        {
            SocketIO.Ins.Signup(param);
        }
        else
        {
            Tip("网络尚未连接，请稍后再试！");
        } 
    }
}
