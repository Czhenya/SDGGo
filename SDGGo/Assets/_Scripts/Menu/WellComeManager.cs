using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SDG;
public class WellComeManager : MonoBehaviour
{

    public InputField username = null;
    public InputField password = null;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Login()
    {
        string uname = username.text;
        string upwd = password.text;
        CurrentPlayer.Ins.user.username = uname;
        CurrentPlayer.Ins.user.password = upwd;
        ParamLogin param = new ParamLogin();
        param.name = uname;

        SocketIO.Ins.Signup(param);

        SceneManager.LoadScene("Menu");
    }
}
