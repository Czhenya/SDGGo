using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG
{
    // 整型坐标数据结构
    public class Point
    {
        public int x;
        public int y;

        public Point(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
    }

    // 棋子数据结构
    public class Move
    {
        public Point pos;
        public int player; // 0:白子 1:黑子 -1:无子
        public int worm;   // 形势值0
        public Move(Point _pos,int _player)
        {
            pos = _pos;
            player = _player;
            worm = 0;
        }
    }

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
            userid = "-1000";
            token = "farhgt7w4t4t4y";
        }
    }

    // 房间信息
    public class RoomInfo {
        public int roomid;
        public string owner;

        public RoomInfo(int _id,string _owner) {
            roomid = _id;
            _owner = _owner;
        }
    }

    public class GOClass
    {

    }
}
