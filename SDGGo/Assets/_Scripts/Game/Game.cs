using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
        
        // 构造函数
        public Game(int _scale, float _borderW)
        {
            // 玩家
            player = 1;
            Players[0] = new Player("白", 0);
            Players[1] = new Player("黑", 1);
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
            Point index = GetCoordIndex(mousePos);
            return GoPanel[index.x, index.y];
        }
        // 根据鼠标精确坐标获取棋盘整型坐标
        public Point GetCoordIndex(Vector2 mousePos)
        {
            int x = (int)Mathf.Round(((mousePos.x - borderW) / gap_width));
            int y = (int)Mathf.Round((mousePos.y - borderH) / gap_height);

            if (x < 0) x = 0;
            if (x >= panelScale) x = panelScale - 1;

            if (y < 0) y = 0;
            if (y >= panelScale) y = panelScale - 1;

            return new Point(x, y);
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
            Point curIndex = GetCoordIndex(mousePosition);
            mat.SetFloat("_mousePosX", GoPanel[curIndex.x, curIndex.y].pos.x);
            mat.SetFloat("_mousePosY", GoPanel[curIndex.x, curIndex.y].pos.y);
            
            // 更新形势数据
            /*
            for (int i = 0; i < panelScale; ++i) {
                string line = "";
                for (int j = 0; j < panelScale; ++j) {
                    int curplayer = GoPanel[j, i].player;
                    int curworm = GoPanel[j, i].worm;
                    float finalworm = -1;
                    switch (curplayer) {
                        case -1:
                            if (curworm > 33) finalworm = 1;
                            if (curworm < -33) finalworm = 0;
                            break;
                        case 0:
                            finalworm = 0;
                            if (curworm > 0) finalworm = 1;
                            break;
                        case 1:
                            finalworm = 1;
                            if (curworm < 0) finalworm = 0;
                            break;
                        default:
                            break;
                    }
                    worms.Add(finalworm);
                    line += "   " + (curworm);
                }
                Debug.Log(line);
            }
            mat.SetFloatArray("_worms", worms);
            */
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

        // 形势更新,黑棋为正，白棋为负，operation表示该点值的变化
        /*
        public void UpdateTerritory(Point newPos,int operation, int iter) {
            bool[] stat = { true, true, true, true, true, true, true, true, true };
            int value = 64 * operation;
            GoPanel[newPos.x, newPos.y].worm += value;
            for (int i = 1; i<iter;++i )
            {
                for (int j = 0; j < 8; ++j) {
                    if (!stat[j]) continue;
                    Point p = GetNeighbor(newPos, 0, j);
                    float power = j < 4 ? 0.5f : 0.25f;
                    int finalValue = (int)(value * Mathf.Pow(power, (float)i));
                    stat[j] = UpdateWorm(p, finalValue);
                }
            }
            int[,] WORM = new int[19,19];
            for (int r = 0; r < 19; ++r) {
                for (int c = 0; c < 19; ++c) {
                    WORM[r, c] = GoPanel[c, r].worm;
                }
            }
        }

        public bool UpdateWorm(Point p, int value) {
            if (IsPointAllowed(p) && player != PlayerToogle())
            {
                GoPanel[p.x, p.y].worm += value;
                return true;
            }
            else {
                return false;
            }
        }

        public Point GetNeighbor(Point start, int dir, int d) {
            switch (dir) {
                case 0:
                    return new Point(start.x, start.y+d);
                    break;
                case 1:
                    return new Point(start.x, start.y-d);
                    break;
                case 2:
                    return new Point(start.x-d, start.y);
                    break;
                case 3:
                    return new Point(start.x+d,start.y);
                    break;
                case 4:
                    return new Point(start.x-d,start.y-d);
                    break;
                case 5:
                    return new Point(start.x - d, start.y + d);
                    break;
                case 6:
                    return new Point(start.x + d, start.y - d);
                    break;
                case 7:
                    return new Point(start.x + d, start.y + d);
                    break;
                default:
                    return start;
            }
        }
        */

    }
}
