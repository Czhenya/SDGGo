using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG
{
    // 用户
    public class User {
        public string username;
        public string password;
        public string userid;
        public string token;
        public int color;       // 棋子颜色

        public User() {
            username = "默认用户名";
            password = "123";
            userid = "-1";
            token = "farhgt7w4t4t4y";
        }
    }

    // 房间信息
    public class RoomInfo {
        public int roomid;
        public string owner;
        public string ownername;

        public RoomInfo(int _id,string _owner, string _ownername) {
            roomid = _id;
            owner = _owner;
            ownername = _ownername;
        }
    }

    public class GOClass
    {

    }
}
