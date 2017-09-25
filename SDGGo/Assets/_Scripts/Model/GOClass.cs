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

    // 玩家对象数据结构
    public class Player 
    {
        public string name;    // 姓名
        public int player;     // 玩家棋子，1为黑子，0为白子
        public int eatStones;  // 提子数

        public Player(string _name, int _player) {
            name = _name;
            player = _player;
        }
    }

    // 用户
    public class User {
        public string username;
        public string password;
        public string userid;
        public string token;
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
