using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

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
    public class Game
    {
        public int gameType;          // 游戏类型：0-人人；1-人机；2-在线对战
        public int gameState;         // 当前游戏状态： 0-游戏未开始；1-游戏中；3-游戏结束
        public int panelScale;        // 棋盘规模
        public int player;            // 当前棋手，1表示黑子，0表示白子
        public int komi;              // 黑子贴目

        public int moveTime;          // 落子时间
        public int timeUsed;          // 游戏已用时间

        public List<Move> Moves = new List<Move>();    // 已下棋子(按照落子顺序)
        public Move[,] GoPanel = new Move[19, 19];     // 整个棋盘棋子二维数组

        // 作为队列结构的open表和close表，用于计算棋串的气
        private List<Move> openList = new List<Move>();
        private List<Point> closedList = new List<Point>();
        private List<Point> closedLibertyList = new List<Point>();

        // 构造函数，初始化游戏实例
        public Game(int _gametype,int _scale)
        {
            player = 1;               // 默认黑子先手
            komi = 9;
            gameType = _gametype;     // 初始化游戏类型
            gameState = 0;            // 初始化游戏状态
            panelScale = _scale;      // 棋盘规模
            moveTime = 10;            // 落子时间限制
            timeUsed = 0;

            // 初始化棋盘棋子对象
            for (int i = 0; i < panelScale; ++i)
            {
                for (int j = 0; j < panelScale; ++j)
                {
                    GoPanel[i, j] = new Move(new Point(i,j),-1);
                }
            }

            // 初始化gnugo
            SDGGoInit(_scale);
        }
        #region 对外接口函数
        // 获取玩家对手
        public int PlayerToogle()
        {
            return (player + 1) % 2;
        }
        // 切换当前玩家
        public void ChangePlayer()
        {
            player = (player + 1) % 2;
        }

        // 坐标是否有效
        public bool IsPointAllowed(Point p)
        {
            return p.x >= 0 && p.x < panelScale && p.y >= 0 && p.y < panelScale;
        }

        // 坐标移除矫正
        public Point PointCorrect(Point p) {
            Point cp = p;
            if (cp.x < 0) cp.x = 0;
            if (cp.x >= panelScale) cp.x = panelScale - 1;
            if (cp.y < 0) cp.y = 0;
            if (cp.y >= panelScale) cp.y = panelScale - 1;
            return cp;
        }

        // 获得指定位置的棋子状态
        public int GetPanelPlayer(Point index)
        {
            if (IsPointAllowed(index))
                return GoPanel[index.x, index.y].player;
            else
                return 404;
        }

        // 自动提子
        public void CheckNoLiberty(Point curPos)
        {
            GoPanel[curPos.x, curPos.y].player = player;
            Point top = new Point(curPos.x, curPos.y + 1);
            Point bottom = new Point(curPos.x, curPos.y - 1);
            Point left = new Point(curPos.x - 1, curPos.y);
            Point right = new Point(curPos.x + 1, curPos.y);

            if (IsPointAllowed(top) && GetPanelPlayer(top) == PlayerToogle())
                EatNoLiberty(top);
            if (IsPointAllowed(bottom) && GetPanelPlayer(bottom) == PlayerToogle())
                EatNoLiberty(bottom);
            if (IsPointAllowed(left) && GetPanelPlayer(left) == PlayerToogle())
                EatNoLiberty(left);
            if (IsPointAllowed(right) && GetPanelPlayer(right) == PlayerToogle())
                EatNoLiberty(right);
        }

        // 撤销上一次提子
        public void RecoverLastDelete()
        {
            for (int i = Moves.Count - 2; ; --i)
            {
                if (Moves[i].removed)
                {
                    Debug.Log("出现提子撤销操作！！！");
                    Moves[i].removed = false;
                    GoUIManager.Ins.recoverMove(Moves[i].pos);
                }
                else
                {
                    return;
                }
            }
        }

        #endregion

        #region 落子函数
        // 逻辑棋盘落子操作
        public bool SetMove(Point index, int color) {
            // 落子合法性
            if (IsOperationAllowed(index) && SetGNUGoMove(index, color))
            {
                // 添加新棋子
                Move newMove = GoPanel[index.x, index.y];
                Moves.Add(newMove);
                return true;
            }
            else
            {
                // 落子失败恢复状态
                GoPanel[index.x, index.y].player = -1;
                Debug.Log("逻辑落子失败！！！");
                return false;
            }
        }

        // 在gnugo棋盘指定位置落子
        public bool SetGNUGoMove(Point index, int color) {
            Point gnup = XY2IJ(index);
            if (SDGPlayMove(gnup.x, gnup.y, color)==1)
                return true;
            Debug.Log("GNUGO逻辑拒绝落子！");
            return false;
        }

        // gnugo智能计算指定颜色最优的落子点落子
        public Point GetGenComputerMove(int color) {
            int genm = SDGGenComputerMove(color);
            Point p = POS2XY(genm);
            return p;
        }

        // 坐标转换
        Point XY2IJ(Point p) { return new Point(panelScale -1 - p.y, p.x); }
        Point IJ2XY(int i, int j) { return new Point(j, panelScale -1 - i); }
        Point POS2XY(int POS) { return IJ2XY(I(POS),J(POS)); }
        int I(int pos) { return ((pos) / (19 + 1) - 1); }
        int J(int pos) { return ((pos) % (19 + 1) - 1); }
#endregion

        #region 游戏算法
        // 判断落子是否合法
        bool IsOperationAllowed(Point index)
        {
            // 1. 是否在棋盘内
            //if (GoUIManager.Ins.isInPanel(mousePos)) return false;

            // 2.位置是否为空
            /*
            
            if (GetPanelPlayer(index) != -1)
            {
                Debug.Log("该位置不为空！气为：" + IsLibertyExist(index) + "closedList" + closedList.Count);
                return false;
            }
            */

            // 3. 所在棋串是否有气
            if (!IsLibertyEnough(index))
            {
                Debug.Log("所在棋串无气，请下在别处！");
                return false;
            }

            return true;
        }

        // 判断气数是否可落子
        bool IsLibertyEnough(Point start)
        {
            // 先更新当前点棋子状态以便下面算法计算
            GoPanel[start.x, start.y].player = player;
            int liberty = IsLibertyExist(start);
            Debug.Log("落子点气数：" + liberty);
            if (liberty > 0)
            {
                return true;
            }
            else
            {
                // 如果气为0还原棋子状态为无子
                GoPanel[start.x, start.y].player = -1;
                return false;
            }
        }

        // 判断当前棋串是否有气
        int IsLibertyExist(Point start)
        {

            // 清空表
            openList.Clear();
            closedList.Clear();
            closedLibertyList.Clear();
            // 起点入队
            openList.Add(GoPanel[start.x, start.y]);
            return GetLiberty(start);
        }
        // 计算当前棋串的气
        int GetLiberty(Point start)
        {
            int liberty = 0;
            // 广度优先搜索（上下左右四邻接点）
            int curplayer = GetPanelPlayer(start);
            LibertyProcess(new Point(start.x, start.y + 1), curplayer, ref liberty);
            LibertyProcess(new Point(start.x, start.y - 1), curplayer, ref liberty);
            LibertyProcess(new Point(start.x - 1, start.y), curplayer, ref liberty);
            LibertyProcess(new Point(start.x + 1, start.y), curplayer, ref liberty);

            // 当前棋子进入closed表
            closedList.Add(openList[0].pos);
            openList.RemoveAt(0);

            if (openList.Count != 0)
            {
                return liberty + GetLiberty(openList[0].pos);
            }
            else
            {
                return liberty;
            }
        }
        // 气
        void LibertyProcess(Point neighbor, int curplayer, ref int liberty)
        {
            if (!IsPointAllowed(neighbor)) return;

            Move move = GoPanel[neighbor.x, neighbor.y];
            int nplayer = GetPanelPlayer(neighbor);
            // 邻接点无子
            if (nplayer == -1 && !IsInCosedList(neighbor, closedLibertyList))
            {
                ++liberty;
                closedLibertyList.Add(neighbor);
            }
            // 邻接点属于相同棋串且不在closedList
            else if (nplayer == curplayer && !IsInCosedList(neighbor, closedList))
            {
                openList.Add(move);
            }
        }

        // 点是否在closedList中
        bool IsInCosedList(Point neighbor, List<Point> closedL)
        {
            for (int i = 0; i < closedL.Count; ++i)
            {
                Point tmp = closedL[i];
                if (tmp.x == neighbor.x && tmp.y == neighbor.y)
                    return true;
            }
            return false;
        }

        // 提子
        void EatNoLiberty(Point start)
        {
            int liberty = IsLibertyExist(start);
            Debug.Log("邻接点气数：" + liberty);
            if (liberty == 0)
            {
                // 提取掉closedList中的棋子
                for (int i = 0; i < closedList.Count; ++i)
                {
                    Debug.Log("断气棋子点" + i + ":(" + closedList[i].x + ", " + closedList[i].y + ")");
                    for (int j = 0; j < Moves.Count; ++j)
                    {
                        // 跳过已经被提掉的棋子
                        if (Moves[j].removed) continue;
                        Point index = Moves[j].pos;
                        if (closedList[i].x == index.x && closedList[i].y == index.y)
                        {
                            // 从已下棋子中移除
                            Moves[j].removed = true;
                            GoUIManager.Ins.deleteMove(index);
                            // 恢复棋盘棋子状态为无子
                            GoPanel[index.x, index.y].player = -1;
                            break;
                        }
                    }
                }

            }
        }
        #endregion

        #region 运行时动态链接库

#if UNITY_EDITOR

        // 测试
        [DllImport("sdggnugo")]
        public static extern int Add(int x, int y);

        // 初始化gnugo
        [DllImport("sdggnugo")]
        public static extern void SDGGoInit(int boardsize);

        // 获取当前打分，正数黑子领先，负数白子领先
        [DllImport("sdggnugo")]
        public static extern float SDGGetScore();

        // 在gnugo棋盘上指定位置落子，并返回是否落子成功
        [DllImport("sdggnugo")]
        public static extern int SDGPlayMove(int i, int j, int color);

        // gnugo落子一步并返回一维落子坐标，如果落子失败返回-1
        [DllImport("sdggnugo")]
        public static extern int SDGGenComputerMove(int color);
        
#else
        
        // 初始化gnugo
        [DllImport("gnuGo-3.8")]
        public static extern void SDGGoInit(int boardsize);

        // 获取当前打分，正数黑子领先，负数白子领先
        [DllImport("gnuGo-3.8")]
        public static extern float SDGGetScore();

        // 在gnugo棋盘上指定位置落子，并返回是否落子成功
        [DllImport("gnuGo-3.8")]
        public static extern int SDGPlayMove(int i, int j, int color);

        // gnugo落子一步并返回一维落子坐标，如果落子失败返回-1
        [DllImport("gnuGo-3.8")]
        public static extern int SDGGenComputerMove(int color);
        
#endif
        #endregion

    }
}