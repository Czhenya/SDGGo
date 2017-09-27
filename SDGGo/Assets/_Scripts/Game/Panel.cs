using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using SDG;

[ExecuteInEditMode]
public class Panel : Singleton<Panel>
{
    public int GameType;                 // 游戏类型：0-人人；1-人机；2-在线对战
    public Image stone;                  // 棋子指示
    //public Text timerLabel;            // 计时标签
    public Text GameResult;              // 当前游戏结算
    public Text GameState;               // 游戏状态
    public Text roomidLabel;             // 房间号标签
    public Text opponentinfo;            // 对手玩家信息
    public Text localplayerinfo;         // 本地玩家信息

    public Game game;                    // 游戏实例
    // Timer timer;                      // 计时器对象
    ParamBase param;

    // 在线对战:
    Point oppenPos = new Point(0, 0);  // 服务器传来的对手落子位置
    bool isOppenMoved = false;         // 对手是否已落子

    #region 脚本生命周期
    // 初始化
    void Start()
    {
        // 初始化
        InitialData();

        // 在线对战初始化
        if (GameType == 2) OnlineInit();
    }

    // 帧回调
    void Update()
    {       
        // 更新玩家信息
        if (CurrentPlayer.Ins) {
            string localcolor = (CurrentPlayer.Ins.user.color == 1) ? "执黑" : "执白";
            localplayerinfo.text = "本地玩家信息：\n" + CurrentPlayer.Ins.user.username + "\n" + localcolor;

            string oppocolor = (CurrentPlayer.Ins.opponent.color == 1) ? "执黑" : "执白";
            opponentinfo.text = "对手玩家信息：\n" + CurrentPlayer.Ins.opponent.username + "\n" + oppocolor;
        }

        // 监听游戏开始
        if (game.gameState == 1) {
            StartGame();
        }

        // 对手落子
        if (isOppenMoved)
        {
            isOppenMoved = false;
            if (game.SetMove(oppenPos, game.player))
            {
                // UI move
                GoUIManager.Ins.setMove(oppenPos, game.player);
                PlayerChange();
            }
        }

    }
    // 固定时间间隔回调
    //void FixedUpdate()
    //{
    //timer.UpdateTimer();
    // }

    // 监听socket回调
    void AddSocketIOListener() {

        // 监听游戏开始
        SocketIO.Ins.sdgSocket.On("RetGameStart", (data) => {
            Dictionary<string, int> dic = JsonConvert.DeserializeObject<Dictionary<string, int>>(data.ToString());
            int room = dic["roomid"];
            Debug.Log("游戏开始！房间号：" + room);
            lock (game)
            {
                Panel.Ins.game.gameState = 1;
            }
        });

        // 监听对手落子
        SocketIO.Ins.sdgSocket.On("RetOperatePiece", (data) =>
        {
            lock (oppenPos)
            {
                Dictionary<string, int> dic = JsonConvert.DeserializeObject<Dictionary<string, int>>(data.ToString());
                Debug.Log("RetOperatePiece:" + data);
                int x = dic["x"];
                int y = dic["y"];
                oppenPos = new Point(x, y);
                isOppenMoved = true;
            }
        });

        // 监听游戏结束
        SocketIO.Ins.sdgSocket.On("RetGameEnd", (data) => {
            lock (game)
            {
                game.gameState = 3; // 游戏结束
                Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString());
                Debug.Log("游戏结束：" + data);
            }
            /*
            "roomid" : 1,
            "type" : 1,
            "winnerid" : 1,
            "winnername" : "zhangsan"
            0: 正常胜利
            1: 有人认输
            2: 有人掉线
             */
        });

        // 监听结算
        SocketIO.Ins.sdgSocket.On("RetCheckOut", (data) => {
            //
        });
    }

    #endregion

    #region 对外接口函数

    // 开始游戏
    public void ReqStartGame() {
        Debug.Log("请求开始游戏");
        string paramstr = JsonConvert.SerializeObject(param);
        SocketIO.Ins.sdgSocket.Emit("ReqGameStart",paramstr);
    }

    // 认输
    public void GiveUpGame()
    {
        ParamGameEnd param = new ParamGameEnd();
        //param.userid = CurrentPlayer
        SocketIO.Ins.sdgSocket.Emit("ReqGameEnd", "");
    }

    // 请求结算
    public void CheckOutGame() {
        string paramstr = JsonConvert.SerializeObject(param);
        SocketIO.Ins.sdgSocket.Emit("ReqCheckOut", paramstr);
    }
    #endregion

    #region 落子操作
    // 人人对战
    bool SetMove_H_H(Point index)
    {
        if (game.SetMove(index, game.player))
        {
            PlayerChange();
            return true;
        }
        else {
            return false;
        }
    }
    // 人机对战
    bool SetMove_H_C(Point index)
    {
        int huamnplayer = 1;
        if (game.player == huamnplayer && game.SetMove(index, huamnplayer))
        {
            PlayerChange();
            StartCoroutine(GNUComputerMove());
            return true;
        }
        else
        {
            return false;
        }
    }
    // 在线对战
    bool SetMove_Online(Point index)
    {
        if (game.player == CurrentPlayer.Ins.user.color)
        {
            if (game.SetMove(index, game.player))
            {
                PlayerChange();
                // 通知服务器
                ParamPlayMove param = new ParamPlayMove();
                param.userid = int.Parse(CurrentPlayer.Ins.user.userid);
                param.token = CurrentPlayer.Ins.user.token;
                param.x = index.x;
                param.y = index.y;
                string paramstr = JsonConvert.SerializeObject(param);
                SocketIO.Ins.sdgSocket.Emit("ReqOperatePiece", paramstr);
                return true;
            }
        }
        return false;
    }
    // 计算机落子
    IEnumerator GNUComputerMove()
    {
        yield return new WaitForSeconds(1);
        int computerplayer = 0;
        Point genm = game.GetGenComputerMove(computerplayer);
        if (game.player == computerplayer && game.SetMove(genm, computerplayer))
        {
            GoUIManager.Ins.setMove(genm, computerplayer);
            PlayerChange();
        }
        else {
            Debug.Log("GNUGo AI 下棋失败！");
        }
        yield return 0;
    }

    // 选子操作
    public bool SelectMove(Point mouseIndex)
    {
        // 非游戏状态
        if (game.gameState != 1) return false;
        bool success = false;

        // 鼠标坐标转换到0-1空间
        switch (game.gameType)
        {
            case 0:
                success = SetMove_H_H(mouseIndex);
                break;
            case 1:
                success = SetMove_H_C(mouseIndex);
                break;
            case 2:
                success = SetMove_Online(mouseIndex);
                break;
            default:
                break;
        }
        return success;
    }
    #endregion

    #region 自定义初始化函数
    // 在线对战初始化
    
    void OnlineInit()
    {
        // 显示房间号
        roomidLabel.text = "房间号：" + CurrentPlayer.Ins.roomId.ToString();
        // 监听
        AddSocketIOListener();
    }
    
    // 初始化
    void InitialData()
    {
        // 初始化游戏
        int scale = GoUIManager.Ins.panelScale;
        game = new Game(GameType, scale);
        
        // 初始化先手棋子颜色
        game.player = 1;
        // 离线游戏直接开始
        if (GameType != 2) StartGame();

        // 客人执黑
        if (CurrentPlayer.Ins.isRoomOwner)
        {
            CurrentPlayer.Ins.user.color = 0;
            CurrentPlayer.Ins.opponent.color = 1;
        }
        else {
            CurrentPlayer.Ins.user.color = 1;
            CurrentPlayer.Ins.opponent.color = 0;
        }
        // 注册计时事件
         //timer = new Timer(game.moveTime);
        // timer.tickEvent += OnTimeEnd;
        // timer.tickSeceondEvent += OnSecond;
        // timerLabel.text = timer._currentTime.ToString();

        // 基本参数
        param = new ParamBase();
        param.userid = int.Parse(CurrentPlayer.Ins.user.userid);
        param.token = CurrentPlayer.Ins.user.token;
    }

    #endregion

    #region 自定义内部函数

    // 游戏开始
    void StartGame()
    {
        game.gameState = 1;
        GameState.text = "游戏开始！";
        //timer.StartTimer();
    }

    // 玩家切换
    void PlayerChange()
    {
        if (game.player == 0)
        {
            stone.color = Color.black;
        }
        else
        {
            stone.color = Color.white;
        }

        // 重置计时器
        //timer.ResetTimer();
       // timer.StartTimer();
       // timerLabel.text = timer._currentTime.ToString();

        UpdateSore();

        game.ChangePlayer();
    }

    // 更新成绩
    void UpdateSore() {
        // 最新成绩结算
        float wscore = Game.SDGGetScore();
        if (wscore > 0)
        {
            GameResult.text = "白棋领先" + wscore + "点！";
        }
        else
        {
            float bscore = -wscore;
            GameResult.text = "黑棋领先" + bscore + "点！";
        }
    }
    #endregion

    #region 计时器逻辑
    void OnTimeEnd()
    {
        // Debug.Log("时间到！");
       // timer.EndTimer();
    }

    void OnSecond()
    {
        //Debug.Log("又一秒！");
        game.timeUsed++;
       // timerLabel.text = timer._currentTime.ToString();
    }
    #endregion

}