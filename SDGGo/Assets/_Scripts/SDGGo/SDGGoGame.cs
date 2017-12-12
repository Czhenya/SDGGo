using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

/*
 *            y
 *             |                    borderH
 *             |__________________________________________________(19,19)
 *           19|                                                  |
 *           18|                                                  |
 *           17|                                                  |
 *           16|       *              *                *          |
 *           15|                                                  |
 *           14|                                                  |
 *           13|                                                  |
 *           12|                                                  |
 *           11|                                                  |
 *  borderW  10|       *              *                *          |  borderW
 *           9 |                                                  |
 *           8 |                                                  |
 *           7 |                                                  |
 *           6 |                                                  |
 *           5 |                                                  |
 *           4 |                                                  |
 *           3 |       *              *                *          |
 *           2 |                                                  |
 *           1 |                                                  |
 *           0 |__________________________________________________|__> x
 *               0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19
 *                                 borderH
 */
namespace SDG {
    public class SDGGoGame
    {
        // 对外暴露的变量
        public int gameType;          // 游戏类型：0-人人；1-人机；2-在线对战
        public int gameState;         // 当前游戏状态： 0-游戏未开始；1-游戏中；3-游戏结束
        public int panelScale;        // 棋盘规模
        public int color;             // color:当前棋手颜色，1表示黑子，0表示白子
        public float komi;            // 黑子贴目

        #region 对外接口函数
        /// <summary>
        /// 构造函数，初始化游戏实例
        /// </summary>
        /// <param name="_gametype">游戏类型（人-人，人-机，在线对战）</param>
        /// <param name="_scale">棋盘规模（19x19)</param>
        public SDGGoGame(int _gametype, int _scale)
        {
            color = 1;               // 默认黑子先手
            komi = 0;
            gameType = _gametype;     // 初始化游戏类型
            gameState = 0;            // 初始化游戏状态
            panelScale = _scale;      // 棋盘规模


            // 初始化gnugo
            SDGGoRuntime.SDGGoInit(_scale);
        }

        /// <summary>
        /// 获取当前玩家对手
        /// </summary>
        /// <returns>对手颜色值</returns>
        public int GetOppenentColor()
        {
            return (color + 1) % 2;
        }
        /// <summary>
        /// 切换当前玩家
        /// </summary>
        public void ChangeColor()
        {
            color = (color + 1) % 2;
        }

        /// <summary>
        /// 坐标是否有效
        /// </summary>
        /// <param name="p">坐标</param>
        /// <returns>是否有效</returns>
        public bool IsPointAllowed(Point p)
        {
            return p.x >= 0 && p.x < panelScale && p.y >= 0 && p.y < panelScale;
        }

        /// <summary>
        /// 坐标溢出矫正
        /// </summary>
        /// <param name="p">坐标</param>
        /// <returns>矫正后的坐标</returns>
        public Point PointCorrect(Point p) {
            Point cp = p;
            if (cp.x < 0) cp.x = 0;
            if (cp.x >= panelScale) cp.x = panelScale - 1;
            if (cp.y < 0) cp.y = 0;
            if (cp.y >= panelScale) cp.y = panelScale - 1;
            return cp;
        }

        /// <summary>
        /// 获得指定位置的棋子状态
        /// </summary>
        /// <param name="index">坐标</param>
        /// <returns>返回棋子位置状态：黑子、白子、空</returns>
        public int GetPanelColor(Point index)
        {
            if (IsPointAllowed(index))
            {
                Point pointij = XY2IJ(index);
                return SDGGoRuntime.SDGBoardStat(pointij.x, pointij.y);
            }
            else
            {
                Debug.Log("坐标不合法，无法获取棋子状态");
                throw new Exception("坐标不合法，无法获取棋子状态");
            }
        }

        /// <summary>
        /// GNUGo逻辑落子
        /// </summary>
        /// <param name="index"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public bool SetMove(Point index, int color)
        {
            Point gnup = XY2IJ(index);
            Debug.Log("落点：i=" + gnup.x + " j=" + gnup.y);
            if (SDGGoRuntime.SDGIsAllowedMove(gnup.x, gnup.y, color) == 1)
            {
                SDGGoRuntime.SDGPlayMove(gnup.x, gnup.y, color);
                return true;
            }
            else
            {
                Debug.Log("GNUGO逻辑拒绝落子!");
                return false;
            }
        }

        /// <summary>
        /// gnugo智能计算指定颜色最优的落子点落子
        /// </summary>
        /// <param name="color">AI棋手的颜色</param>
        /// <returns>AI想到的落子位置</returns>
        public Point GetGenComputerMove(int color)
        {
            int genm = SDGGoRuntime.SDGGenComputerMove(color);
            Point p = POS2XY(genm);
            return p;
        }

        /// <summary>
        /// 获取当前得分
        /// </summary>
        /// <returns>分值，大于0说明白棋领先，反之黑棋领先</returns>
        public float GetCurrentScore() {
            return SDGGoRuntime.SDGGetScore();
        }
        #endregion

        #region 游戏工具算法
        // 坐标转换
        Point XY2IJ(Point p) { return new Point(panelScale - 1 - p.y, p.x); }
        Point IJ2XY(int i, int j) { return new Point(j, panelScale - 1 - i); }
        Point POS2XY(int POS) { return IJ2XY(I(POS), J(POS)); }
        int I(int pos) { return ((pos) / (19 + 1) - 1); }
        int J(int pos) { return ((pos) % (19 + 1) - 1); }

        #endregion
    }
}