using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using SDG;

[ExecuteInEditMode]
public class Panel : Singleton<Panel>
{
    public int GameType;               // 游戏类型：0-人人；1-人机；2-在线对战
    public Image stone;                // 棋子指示
    public Text timerLabel;            // 计时标签
    public Text ScoreLabel;            // 打分标签
    public Text roomidLabel;           // 房间号标签
    public Text[] playerText;          // 玩家信息
    public Text localplayerinfo;       // 本地玩家信息

    public Game game;                 // 游戏实例
    private Timer timer;               // 计时器对象

    // 在线对战:
    int localPlayer = 1;               // 本地棋手颜色
    Point oppenPos = new Point(0, 0);   // 服务器传来的对手落子位置
    bool isOppenMoved = false;         // 对手是否已落子

    #region 脚本生命周期
    // 初始化
    void Start()
    {
        InitialData();
        StartGame();
    }

    // 帧回调
    void Update()
    {
        stone.color = game.player == 1 ? Color.black : Color.white;
        string localcolor = localPlayer == 1 ? "执黑" : "执白";
        localplayerinfo.text = "本地玩家信息：\n" + CurrentPlayer.Ins.user.username + "\n" + localcolor;

        // 对手落子
        if (isOppenMoved)
        {
            isOppenMoved = false;
            if (game.SetMove(oppenPos, game.player))
            {
                // UI move
                game.PlayerChange();
            }
        }
    }
    // 固定时间间隔回调
    void FixedUpdate()
    {
        // timer.UpdateTimer();
    }

    #endregion

    #region 对外接口函数

    // 落子操作
    // 人人对战
    bool SetMove_H_H(Point index)
    {
        if (game.SetMove(index, game.player))
        {
            game.PlayerChange();
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
            game.PlayerChange();
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
        if (game.player == localPlayer)
        {
            if (game.SetMove(index, game.player))
            {
                game.PlayerChange();
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
            game.PlayerChange();
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
        // 最新成绩结算
        float wscore = Game.SDGGetScore();
        if (wscore > 0)
        {
            ScoreLabel.text = "白棋领先" + wscore + "点！";
        }
        else
        {
            float bscore = -wscore;
            ScoreLabel.text = "黑棋领先" + bscore + "点！";
        }
        return success;
    }
    #endregion

    #region 自定义初始化函数
    // 在线对战初始化
    
    void OnlineInit()
    {
        // 房主先手
        if (CurrentPlayer.Ins.isRoomOwner)
        {
            game.Players[0] = new Player(CurrentPlayer.Ins.name, 1); // 房主执黑
            localPlayer = 1;
        }
        else
        {
            game.Players[1] = new Player(CurrentPlayer.Ins.name,0);  // 客人执白
            localPlayer = 0;
        }
        roomidLabel.text = "房间号：" + CurrentPlayer.Ins.roomId.ToString();

        // 监听对手落子
        SocketIO.Ins.sdgSocket.On("RetOperatePiece", (data) =>
        {
            lock (oppenPos)
            {
                oppenPos = new Point(0,0);
                isOppenMoved = true;
            }
        });
    }
    
    // 初始化
    
    void InitialData()
    {
        // 初始化游戏
        int scale = GoUIManager.Ins.panelScale;
        game = new Game(GameType, scale);
        // 在线对战
        if (GameType == 2) OnlineInit();
        // 初始化先手棋子颜色
        game.player = 1;

        // 注册计时事件
        /* timer = new Timer(game.moveTime);
         timer.tickEvent += OnTimeEnd;
         timer.tickSeceondEvent += OnSecond;
         timerLabel.text = timer._currentTime.ToString();
         */
    }
    


    #endregion

    #region 自定义内部函数

    // 游戏开始
    void StartGame()
    {
        game.gameState = 1;
        // timer.StartTimer();
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
        game.PlayerChange();

        /*
        timer.ResetTimer();
        timer.StartTimer();
        timerLabel.text = timer._currentTime.ToString();
        */
    }
    
    #endregion

    #region 计时器逻辑
    void OnTimeEnd()
    {
        // Debug.Log("时间到！");
        timer.EndTimer();
    }

    void OnSecond()
    {
        //Debug.Log("又一秒！");
        game.timeUsed++;
        timerLabel.text = timer._currentTime.ToString();
    }
#endregion
}