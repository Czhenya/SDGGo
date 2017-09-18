using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

namespace SDG {
    [ExecuteInEditMode]
    public class Panel : Singleton<Panel>
    {
        // UI
        public Material mat;
        public Image stone;
        public Text[] playerText;
        public Text timerLabel;

        // 作为队列结构的open表和close表，用于计算棋串的气
        private List<Move> openList = new List<Move>();
        private List<Point> closedList = new List<Point>();
        private List<Point> closedLibertyList = new List<Point>();

        private Game game;                // 游戏对象
        private Timer timer;              // 计时器对象

        #region 脚本生命周期
        // 初始化
        void Start()
        {
            InitialData();
            InitialAllShaderData();
            SDGGoInit(19);
            Debug.Log("1+2="+Add(1,2));
        }

        // 帧回调
        void Update()
        {

        }
        // 固定时间间隔回调
        void FixedUpdate() {
            timer.UpdateTimer();
        }

        // 绘制shader材质
        void OnRenderImage(RenderTexture src, RenderTexture des)
        {
            Graphics.Blit(src, des, mat);
        }

        void OnDestroy() {
            // 注销计时事件
            //timer.tickEvent -= OnTimeEnd;
            //timer.tickSeceondEvent -= OnSecond;
        }

        #endregion

        #region 对外接口函数

        // 落子操作
        public void SetMove()
        {
            Point index = game.GetCoordIndex(game.mousePosition);
            // 更新棋盘棋子状态
            int curplayer = game.GoPanel[index.x, index.y].player;
            // 只能落在无子位置
            if (curplayer != -1) return;
            game.GoPanel[index.x, index.y].player = game.player;
            // 尝试提子
            CheckNoLiberty(index);
            // 落子合法性
            if (IsOperationAllowed(game.mousePosition))
            {
                Point gnup = XY2IJ(index);
                if (!SDGPlayMove(index.x,index.y, game.player)) return;
                Debug.Log(SDGGetScore());

                // 添加新棋子
                Move newMove = game.GoPanel[index.x, index.y];
                game.Moves.Add(newMove);

                // 更新形势
                //int operation = game.player == 0 ? -1 : 1;
                // game.UpdateTerritory(index,operation,4);
                // 传数据给shader
                game.UpdateShader(ref mat);
                // 玩家切换
                PlayerChange();
            }
            else
            {
                // 落子失败恢复状态
                game.GoPanel[index.x, index.y].player = curplayer;
            }
        }
        // 选子操作
        public void SelectMove(Vector2 mousePos) {
            // 鼠标坐标转换到0-1空间
            Vector2 curMousePos = new Vector2(mousePos.x / (float)Screen.width, mousePos.y / (float)Screen.height);
            Point curIndex = game.GetCoordIndex(curMousePos);
            Point preIndex = game.GetCoordIndex(game.mousePosition);
            // 判断第二次点击确认
            if (curIndex.x == preIndex.x && curIndex.y == preIndex.y)
            {
                SetMove();
            }
            else {
                game.mousePosition = curMousePos;
            }
        }

        // 悔棋一步
        public void MoveBack() {
            if (game.Moves.Count > 0) {
                // 恢复棋子位置为无子
                Point index = game.GetCoordIndex(game.Moves[game.Moves.Count - 1].pos);
                game.GoPanel[index.x, index.y].player = -1;
                // 移除棋子
                game.Moves.RemoveAt(game.Moves.Count - 1);
                game.UpdateShader(ref mat);
                // 玩家切换
                PlayerChange();
            }
        }
        #endregion

        #region 自定义初始化函数
        // 初始化
        void InitialData()
        {
            // 黑子先手
            stone.color = Color.black;
            // 初始化游戏
            int scale = mat.GetInt("_panelScale");
            float borderW = mat.GetFloat("_borderWidth");
            game = new Game(scale, borderW);
            playerText[0].text = game.Players[0].name;
            playerText[1].text = game.Players[1].name;

            // 注册计时事件
            timer = new Timer(game.moveTime);
            timer.tickEvent += OnTimeEnd;
            timer.tickSeceondEvent += OnSecond;
            timer.StartTimer();
            timerLabel.text = timer._currentTime.ToString();
        }

        // 初始化shader待传数据
        void InitialAllShaderData()
        {
            List<Vector4> v4s = new List<Vector4>();
            // 传入九颗星的坐标给shader
            for (int i = 0; i < 9; ++i)
            {
                v4s.Add(new Vector4(game.stars[i].pos.x, game.stars[i].pos.y, 0, 0));
            }
            mat.SetVectorArray("_Stars", v4s);

            v4s.Clear();
            for (int i = 0; i < game.panelScale * game.panelScale; ++i)
            {
                v4s.Add(new Vector4(0.1f, 0.1f, 0.1f, 0.1f));
            }
            mat.SetInt("_StepsWhite", 0);
            mat.SetInt("_StepsBlack", 0);
            mat.SetVectorArray("_MovesBlack", v4s);
            mat.SetVectorArray("_MovesWhite", v4s);
            mat.SetInt("_lastPlayer", 0);
            mat.SetFloat("_mousePosX", -0.5f);
            mat.SetFloat("_mousePosY", -0.5f);
        }
        #endregion

        #region 自定义内部函数

        Point XY2IJ(Point p) {
            return new Point(game.panelScale - p.y, p.x);
        }

        // 玩家切换
        void PlayerChange() {
            if (game.player == 0)
            {
                stone.color = Color.black;
            }
            else
            {
                stone.color = Color.white;
            }
            game.PlayerChange();

            timer.ResetTimer();
            timer.StartTimer();
            timerLabel.text = timer._currentTime.ToString();
        }
        #endregion

        #region 游戏算法
        // 判断落子是否合法
        bool IsOperationAllowed(Vector2 mousePos)
        {
            // 1. 是否在棋盘内
            if (!game.IsInPanel(mousePos)) return false;

            // 2.位置是否为空
            /*
            
            if (GetPanelPlayer(index) != -1)
            {
                Debug.Log("该位置不为空！气为：" + IsLibertyExist(index) + "closedList" + closedList.Count);
                return false;
            }
            */

            // 3. 所在棋串是否有气
            Point index = game.GetCoordIndex(mousePos);
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
            game.GoPanel[start.x, start.y].player = game.player;
            int liberty = IsLibertyExist(start);
            Debug.Log("落子点气数：" + liberty);
            if (liberty > 0)
            {
                return true;
            }
            else
            {
                // 如果气为0还原棋子状态为无子
                game.GoPanel[start.x, start.y].player = -1;
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
            openList.Add(game.GoPanel[start.x, start.y]);
            return GetLiberty(start);
        }
        // 计算当前棋串的气
        int GetLiberty(Point start)
        {
            int liberty = 0;
            // 广度优先搜索（上下左右四邻接点）
            int curplayer = game.GetPanelPlayer(start);
            LibertyProcess(new Point(start.x, start.y + 1), curplayer, ref liberty);
            LibertyProcess(new Point(start.x, start.y - 1), curplayer, ref liberty);
            LibertyProcess(new Point(start.x - 1, start.y), curplayer, ref liberty);
            LibertyProcess(new Point(start.x + 1, start.y), curplayer, ref liberty);

            // 当前棋子进入closed表
            closedList.Add(game.GetCoordIndex(openList[0].pos));
            openList.RemoveAt(0);

            if (openList.Count != 0)
            {
                return liberty + GetLiberty(game.GetCoordIndex(openList[0].pos));
            }
            else
            {
                return liberty;
            }
        }
        // 气
        void LibertyProcess(Point neighbor, int curplayer, ref int liberty)
        {
            if (!game.IsPointAllowed(neighbor)) return;

            Move move = game.GoPanel[neighbor.x, neighbor.y];
            int nplayer = game.GetPanelPlayer(neighbor);
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

            if (game.IsPointAllowed(top) && game.GetPanelPlayer(top) == game.PlayerToogle())
                EatNoLiberty(top);
            if (game.IsPointAllowed(bottom) && game.GetPanelPlayer(bottom) == game.PlayerToogle())
                EatNoLiberty(bottom);
            if (game.IsPointAllowed(left) && game.GetPanelPlayer(left) == game.PlayerToogle())
                EatNoLiberty(left);
            if (game.IsPointAllowed(right) && game.GetPanelPlayer(right) == game.PlayerToogle())
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
                    for (int j = 0; j < game.Moves.Count; ++j)
                    {
                        Point index = game.GetCoordIndex(game.Moves[j].pos);
                        if (closedList[i].x == index.x && closedList[i].y == index.y)
                        {
                            // 从已下棋子中移除
                            game.Moves.RemoveAt(j);
                            // 恢复棋盘棋子状态为无子
                            game.GoPanel[index.x, index.y].player = -1;
                            break;
                        }
                    }
                }

            }
        }
        #endregion

        #region 计时逻辑
        void OnTimeEnd() {
            // Debug.Log("时间到！");
            timer.EndTimer();
        }

        void OnSecond() {
            //Debug.Log("又一秒！");
            game.timeUsed++;
            timerLabel.text = timer._currentTime.ToString();
        }

        #endregion

        #region 运行时动态链接库
        [DllImport("sdggnugo")]
        public static extern int Add(int x,int y);

        [DllImport("sdggnugo")]
        public static extern void SDGGoInit(int boardsize);
        [DllImport("sdggnugo")]
        public static extern float SDGGetScore();
        [DllImport("sdggnugo")]
        public static extern bool SDGPlayMove(int i, int j, int color);
        #endregion
    }
}
