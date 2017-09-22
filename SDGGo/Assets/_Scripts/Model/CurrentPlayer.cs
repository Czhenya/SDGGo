using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDG;

    public class CurrentPlayer:Singleton<CurrentPlayer>
    {
        public User user;             // 用户信息
        public int roomId;            // 房间号
        public bool isRoomOwner;      // 是否是房主

    private void Start()
    {
        user = new User();
        roomId = 0;
        isRoomOwner = false;
    }
}