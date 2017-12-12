using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG {
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
    /*
    public class Move
    {
        public Point pos;
        public int color;   // 0:白子 1:黑子 -1:无子
        public int worm;     // 形势值0
        public bool removed; // 是否被提掉
        public Move(Point _pos, int _color)
        {
            pos = _pos;
            color = _color;
            worm = 0;
            removed = false;
        }
    }*/
}

public class BaseModel : MonoBehaviour {

}
