using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 棋局中玩家信息UI模型
/// </summary>
[Serializable]
public class UIPlayer {
    public Image avatar;
    public Text name;
    public Image color;
    public Text state;
}

/// <summary>
/// 游戏界面UI模型
/// </summary>
[Serializable]
public class UIGame {
    public GameObject object_gameui;
    public GameObject object_startgame;             // 开始游戏按钮
    public Text text_timer;              // 计时标签
    public Text text_GameResult;              // 当前游戏结算
    public Text text_GameState;               // 游戏状态
    public Text text_roomid;             // 房间号标签
    public GameObject checkoutDialog;
    public GameObject[] colorsToMove;
    public UIPlayer[] players;
}

/// <summary>
/// 游戏结束界面UI模型
/// </summary>
[Serializable]
public class UIGameEnd {
    public GameObject object_gameendui;
    public GameObject[] object_gameendicon;
    public Text text_gameendresult;

}



public class UIClass {

}
