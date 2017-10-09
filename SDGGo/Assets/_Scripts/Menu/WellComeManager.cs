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

    void Update()
    {
        // 主线程更新UI
        switch (curStat) {
            case 0: Tip(SocketIO.Ins.tiptext);break;
            case 1: Tip("登录成功！"); EnterMenuScene(); break;
            case 2: Tip("登录失败！"); break;
            case 3: Tip("注册成功！"); break;
            case 4: Tip("注册失败！"); break;
            default:Tip(SocketIO.Ins.tiptext); break;
        }

        if (Input.GetMouseButtonDown(1)) {
            SceneManager.LoadScene("Menu");
        }
    }

    // 提示
    void Tip(string msg) {
        tip.text = msg;
    }

    /// <summary>
    ///  登录
    /// </summary>
    public void Login()
    {        
        SocketIO.Ins.OpenSocket();
        string uname = username.text;
        string upwd = password.text;
        CurrentPlayer.Ins.user.username = uname;
        CurrentPlayer.Ins.user.password = upwd;
        ParamBase param = new ParamBase();
        param.name = uname;
        string paramstr = JsonConvert.SerializeObject(param);
        SocketIO.Ins.sdgSocket.Emit("ReqSignIn", paramstr);
    }

    /// <summary>
    /// 注册
    /// </summary>
    public void SignUp() {
        
        SocketIO.Ins.OpenSocket();
        string uname = username.text;
        string upwd = password.text;
        ParamBase param = new ParamBase();
        param.name = uname;
        string paramstr = JsonConvert.SerializeObject(param);
        SocketIO.Ins.sdgSocket.Emit("ReqSignUp", paramstr);
    }

    // 进入主页面
    public void EnterMenuScene() {
        SceneManager.LoadScene("Menu");
    }
}