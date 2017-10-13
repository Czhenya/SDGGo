using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDG;

public class CurrentPlayer : Singleton<CurrentPlayer>
{
    public User user;             // 用户信息

    // 在线游戏全局数据
    public User opponent;         // 对手信息
    public string roomId;         // 房间号
    public int player_num;        // 房间人数
    public bool isRoomOwner;      // 是否是房主
    public string winner_id;

    void Start()
    {
        user = new User();
        user.userid = "643789";
        user.color = 1;
        opponent = new User();
        opponent.userid = "649869";
        opponent.color = 0;
        Reset();
    }

    public void Reset()
    {
        roomId = "";
        isRoomOwner = false;         // 默认不是房主
        winner_id = "-1";
        player_num = 0;
    }
}