using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

/*
 *  y
 *   |
 *   |__________________________________________________(19,19)
 * 19|                                                  |
 * 18|                                                  |
 * 17|                                                  |
 * 16|       *              *                *          |
 * 15|                                                  |
 * 14|                                                  |
 * 13|                                                  |
 * 12|                                                  |
 * 11|                                                  |
 * 10|       *              *                *          |
 * 9 |                                                  |
 * 8 |                                                  |
 * 7 |                                                  |
 * 6 |                                                  |
 * 5 |                                                  |
 * 4 |                                                  |
 * 3 |       *              *                *          |
 * 2 |                                                  |
 * 1 |                                                  |
 * 0 |__________________________________________________|__> x
 *     0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19
 */

namespace SDG {
    public class Game
    {
        public int gameType;          // 游戏类型：0-人人；1-人机；2-在线对战
        public int panelScale;        // 棋盘规模
        public int player;            // 当前棋手，1表示黑子，0表示白子
        public float borderW;         // 棋盘水平边界
        public float borderH;         // 棋盘竖直边界
        public float panelWidth;      // 棋盘宽度
        public float panelHeight;     // 棋盘高度
        public float aspectRatio;     // 屏幕宽高比
        public float gap_height;      // 竖直方向棋子间隔
        public float gap_width;       // 水平方向棋子间隔


        public int moveTime;          // 落子时间
        public int timeUsed;          // 游戏已用时间
        public Vector2 mousePosition; // 当前鼠标点击坐标

        public Player[] Players = new Player[2];       // 两个玩家
        public List<Move> Moves = new List<Move>();    // 已下棋子(按照落子顺序)
        public Move[,] GoPanel = new Move[19, 19];     // 整个棋盘棋子二维数组
        public Move[] stars = new Move[9];             // 九星棋子集合

        // 作为队列结构的open表和close表，用于计算棋串的气
        private List<Move> openList = new List<Move>();
        private List<Point> closedList = new List<Point>();
        private List<Point> closedLibertyList = new List<Point>();

        // 构造函数
        public Game(int _gametype,int _scale, float _borderW)
        {
            // 玩家
            player = 1;
            Players[0] = new Player("白", 0);
            Players[1] = new Player("黑", 1);
            gameType = _gametype;
            // 棋盘规模
            panelScale = _scale;
            // 屏幕宽高比
            aspectRatio = (float)Screen.width / (float)Screen.height;
            // 从shader获取棋盘横向边界
            borderW = _borderW;
            // 计算棋盘宽度
            panelWidth = 1 - 2 * borderW;
            // 棋盘高度
            panelHeight = panelWidth * aspectRatio;
            // 棋盘纵向边界
            borderH = (1 - panelHeight) / 2;
            // 棋子间隔
            gap_height = panelHeight / (panelScale - 1);
            gap_width = panelWidth / (panelScale - 1);

            moveTime = 10;
            timeUsed = 0;

            // 初始化棋盘棋子对象
            for (int i = 0; i < panelScale; ++i)
            {
                for (int j = 0; j < panelScale; ++j)
                {
                    Vector2 pos = new Vector2(borderW + i * gap_width, borderH + j * gap_height);
                    GoPanel[i, j] = new Move(pos, -1);
                }
            }

            // 九星
            int index = 0;
            for (int i = 3; i <= 15; i += 6)
            {
                for (int j = 3; j < 16; j += 6)
                {
                    stars[index] = GoPanel[i, j];
                    ++index;
                }
            }

            // 初始化gnugo
            SDGGoInit(_scale);
        }

        // 获取玩家对手
        public int PlayerToogle()
        {
            return (player + 1) % 2;
        }
        // 切换当前玩家
        public void PlayerChange()
        {
            player = (player + 1) % 2;
        }

        // 坐标是否有效
        public bool IsPointAllowed(Point p)
        {
            return p.x >= 0 && p.x < panelScale && p.y >= 0 && p.y < panelScale;
        }

        // 根据棋盘整型坐标获取棋盘棋子精确坐标
        public Move GetCoord(Point index)
        {
            return GoPanel[index.x, index.y];
        }
        // 根据鼠标精确坐标获取棋盘棋子精确坐标
        public Move GetCoord(Vector2 mousePos)
        {
            Point index = Position2Index(mousePos);
            return GoPanel[index.x, index.y];
        }
        // 根据鼠标精确坐标获取棋盘整型坐标
        public Point Position2Index(Vector2 mousePos)
        {
            int x = (int)Mathf.Round(((mousePos.x - borderW) / gap_width));
            int y = (int)Mathf.Round((mousePos.y - borderH) / gap_height);

            if (x < 0) x = 0;
            if (x >= panelScale) x = panelScale - 1;

            if (y < 0) y = 0;
            if (y >= panelScale) y = panelScale - 1;

            return new Point(x, y);
        }

        // 整形坐标转精确坐标
        public Vector2 Index2Position(Point index) {
            if (index.x < 0 || index.x >= panelScale || index.y < 0 || index.y > panelScale) return new Vector2(0,0);

            float x = borderW + index.x * gap_width;
            float y = borderH + index.y * gap_height;

            return new Vector2(x,y);
        }

        // 获得指定位置的棋子状态
        public int GetPanelPlayer(Point index)
        {
            if (IsPointAllowed(index))
                return GoPanel[index.x, index.y].player;
            else
                return 404;
        }

        // 更新数据给shader
        public void UpdateShader(ref Material mat)
        {
            List<Vector4> moves_black = new List<Vector4>();
            List<Vector4> moves_white = new List<Vector4>();

            List<float> worms = new List<float>();
            for (int i = 0; i < Moves.Count; ++i)
            {
                if (Moves[i].player == 1)
                {
                    moves_black.Add(new Vector4(Moves[i].pos.x, Moves[i].pos.y, 0, 0));
                }
                else
                {
                    moves_white.Add(new Vector4(Moves[i].pos.x, Moves[i].pos.y, 0, 0));
                }
            }
            if (moves_black.Count > 0)
            {
                mat.SetVectorArray("_MovesBlack", moves_black);
            }
            if (moves_white.Count > 0)
            {
                mat.SetVectorArray("_MovesWhite", moves_white);
            }
            mat.SetInt("_lastPlayer", player + 1);
            mat.SetInt("_StepsBlack", moves_black.Count);
            mat.SetInt("_StepsWhite", moves_white.Count);

            // 鼠标点击棋子坐标
            Point curIndex = Position2Index(mousePosition);
            mat.SetFloat("_mousePosX", GoPanel[curIndex.x, curIndex.y].pos.x);
            mat.SetFloat("_mousePosY", GoPanel[curIndex.x, curIndex.y].pos.y);
        }

        // 判断下子操作是否在棋盘区域
        public bool IsInPanel(Vector2 mousePos)
        {
            if (mousePos.x > (borderW - gap_width) && mousePos.x < (1 - borderW + gap_width) && mousePos.y > (borderH - gap_height) && mousePos.y < (1 - borderH + gap_height))
            {
                return true;
            }
            else
            {
                Debug.Log("请在棋盘内下棋！");
                return false;
            }
        }

        // 本地棋盘落子操作
        public bool SetMove(Point index, int color) {
            // 更新棋盘棋子状态
            int curplayer = GoPanel[index.x, index.y].player;
            // 只能落在无子位置
            if (curplayer != -1) return false;
            GoPanel[index.x, index.y].player = color;
            // 尝试提子
            CheckNoLiberty(index);
            // 落子合法性
            if (IsOperationAllowed(mousePosition))
            {
                if (!SetGNUGoMove(index,color)) return false;
                // 添加新棋子
                Move newMove = GoPanel[index.x, index.y];
                Moves.Add(newMove);
                return true;
            }
            else
            {
                // 落子失败恢复状态
                GoPanel[index.x, index.y].player = curplayer;
                return false;
            }
        }

        // 本地当前鼠标位置指定颜色落子
        public bool SetMove(int color)
        {
            Point index = Position2Index(mousePosition);
            return SetMove(index, color);
        }

        // 在gnugo棋盘指定位置落子
        public bool SetGNUGoMove(Point index, int color) {
            Point gnup = XY2IJ(index);
            if (SDGPlayMove(gnup.x, gnup.y, color))
                return true;
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

        #region 游戏算法
        // 判断落子是否合法
        bool IsOperationAllowed(Vector2 mousePos)
        {
            // 1. 是否在棋盘内
            if (!IsInPanel(mousePos)) return false;

            // 2.位置是否为空
            /*
            
            if (GetPanelPlayer(index) != -1)
            {
                Debug.Log("该位置不为空！气为：" + IsLibertyExist(index) + "closedList" + closedList.Count);
                return false;
            }
            */

            // 3. 所在棋串是否有气
            Point index = Position2Index(mousePos);
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
            closedList.Add(Position2Index(openList[0].pos));
            openList.RemoveAt(0);

            if (openList.Count != 0)
            {
                return liberty + GetLiberty(Position2Index(openList[0].pos));
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

        // 自动提子
        void CheckNoLiberty(Point curPos)
        {
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
                        Point index = Position2Index(Moves[j].pos);
                        if (closedList[i].x == index.x && closedList[i].y == index.y)
                        {
                            // 从已下棋子中移除
                            Moves.RemoveAt(j);
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
        public static extern bool SDGPlayMove(int i, int j, int color);

        // gnugo落子一步并返回一维落子坐标，如果落子失败返回-1
        [DllImport("sdggnugo")]
        public static extern int SDGGenComputerMove(int color);
        #endregion

    }
}