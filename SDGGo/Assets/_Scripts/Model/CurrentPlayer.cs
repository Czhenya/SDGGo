using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG {
    public class CurrentPlayer : Singleton<CurrentPlayer>
    {
        public User user;             // 用户信息
        public int roomNumber;        // 房间号
        public bool isRoomManager;    // 是否是房主
    }
}