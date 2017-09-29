using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDG;

    public class CurrentPlayer:Singleton<CurrentPlayer>
    {
        public User user;             // 用户信息

        // 在线游戏全局数据
        public User opponent;         // 对手信息
        public string roomId;         // 房间号
        public bool isRoomOwner;      // 是否是房主
        public bool isWinner;         // 是否获胜

    void Start()
    {
        user = new User();
        opponent = new User();
        roomId = "";
        isRoomOwner = false;         // 默认不是房主
        isWinner = false;
    }
}