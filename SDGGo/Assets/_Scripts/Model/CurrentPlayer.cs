using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDG;

public class CurrentPlayer : Singleton<CurrentPlayer>
{

    // 在线游戏全局数据
    public User user;             // 用户信息
    public User opponent;         // 对手信息
    public string roomId;         // 房间号
    public int player_num;        // 房间人数
    public bool isRoomOwner;      // 是否是房主
    public string winner_id;      // 获胜者id

    // 游戏设置
    public bool isBGMOn;          // 背景音乐开关
    public bool isKomi;           // 黑子是否贴目
    public bool isAIPlayerFisrt;  // 人机对战是否玩家先手

    void Start()
    {
        user = new User();
        user.userid = "643789";
        user.color = 1;
        opponent = new User();
        opponent.userid = "649869";
        opponent.color = 0;
        Reset();

        isBGMOn = true;
        isKomi = true;
        isAIPlayerFisrt = true;
    }

    public void Reset()
    {
        roomId = "";
        isRoomOwner = false;         // 默认不是房主
        winner_id = "-1";
        player_num = 0;
    }
}