using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using SDG;

[ExecuteInEditMode]
public class Panel : Singleton<Panel>
{
    public int GameType;                 // 游戏类型(编辑器序列化传入)：0-人人；1-人机；2-在线对战

    public UIGame gameui;
    public UIGameEnd gameendui;

    public Game game = null;                    // 游戏实例
    //Timer timer;                       // 计时器对象
    ParamBase param;

    // 在线对战:
    object locker = new object();
    Point oppenPos = new Point(0, 0);    // 服务器传来的对手落子位置
    bool isOppenMoved = false;           // 对手是否已落子开关
    bool isGameStart = false;             // 游戏开始开关
    bool isGameOver = false;             // 游戏结束开关
    bool isReqCheckOut = false;          // 请求结算开关

    #region 脚本生命周期
    // 初始化
    void Start()
    {
        ReStartGame();
    }

    // 帧回调
    void Update()
    {       
        // 更新玩家信息
        UIPlayer local = gameui.players[CurrentPlayer.Ins.user.color];
        UIPlayer oppoent = gameui.players[CurrentPlayer.Ins.opponent.color];
        // 名字
        local.name.text = CurrentPlayer.Ins.user.username;
        oppoent.name.text = CurrentPlayer.Ins.opponent.username;
        // 状态
        gameui.players[game.player].state.text = "落子中...";
        gameui.players[game.PlayerToogle()].state.text = "";

        // 游戏开始
        if (isGameStart) {
            isGameStart = false;
            StartGame();
        }

        // 游戏结束
        if (isGameOver) {
            isGameOver = false;
            GameOver();
        }

        // 对手落子
        if (isOppenMoved)
        {
            isOppenMoved = false;
            // 界面落子
            GoUIManager.Ins.setMove(oppenPos, game.player);
            // 逻辑落子
            if (game.SetMove(oppenPos, game.player))
            {
                PlayerChange();
            }
            else {
                Debug.Log("对手落子异常！");
            }
        }

        // 请求结算
        if (isReqCheckOut) {
            isReqCheckOut = false;
            gameui.checkoutDialog.SetActive(true);
        }

    }

    // 固定时间间隔回调
    void FixedUpdate()
    {
    //timer.UpdateTimer();
    }

    // 监听socket回调
    void AddSocketIOListener() {

        // 监听游戏开始
        SocketIO.Ins.sdgSocket.On("RetGameStart", (data) => {
            Dictionary<string, int> dic = JsonConvert.DeserializeObject<Dictionary<string, int>>(data.ToString());
            int room = dic["roomid"];
            Debug.Log("游戏开始！房间号：" + room);
            lock (locker)
            {
                isGameStart = true;
            }
        });

        // 监听对手落子
        SocketIO.Ins.sdgSocket.On("RetOperatePiece", (data) =>
        {
            lock (locker)
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
                Dictionary<string, object> dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ToString());
                Debug.Log("游戏结束：" + data);
                int room_id = int.Parse(dic["roomid"].ToString());
                int type = int.Parse(dic["type"].ToString()); /* 0: 正常胜利 1: 有人认输 2: 有人掉线 */
                string winner_id = dic["winnerid"].ToString();
                string winner_name = dic["winnername"].ToString();
                CurrentPlayer.Ins.winner_id = winner_id;
                isGameOver = true;
            }
        });

        // 监听结算
        SocketIO.Ins.sdgSocket.On("RetCheckOut", (data) => {
            // 弹出确认结算对话框
            Debug.Log("确认结算通知！");
            lock (locker) {
                isReqCheckOut = true;
            }
        });
    }

    #endregion

    #region 对外接口函数

    // 开始游戏
    public void ReqStartGame() {
        if (GameType != 2) return;
        Debug.Log("请求开始游戏");
        string paramstr = JsonConvert.SerializeObject(param);
        SocketIO.Ins.sdgSocket.Emit("ReqGameStart",paramstr);
    }

    // 游戏结束
    void GameOver() {
        // 游戏结束
        game.gameState = 3;
        ShowWinnerInfo();
        CurrentPlayer.Ins.Reset();
    }

    // 认输
    public void GiveUpGame()
    {
        if (GameType == 0) return; // 离线人-人不可认输

        // 游戏结束
        CurrentPlayer.Ins.winner_id = CurrentPlayer.Ins.opponent.userid;
        GameOver();

        if (GameType == 1) return; // 人机不需要通知服务器

        // 在线对战通知服务器有人认输
        ParamGameEnd param = new ParamGameEnd();
        param.userid = int.Parse(CurrentPlayer.Ins.user.userid);
        param.token = CurrentPlayer.Ins.user.token;
        param.winnerid = int.Parse(CurrentPlayer.Ins.opponent.userid);
        param.type = 1;// 认输
        string paramstr = JsonConvert.SerializeObject(param);
        SocketIO.Ins.sdgSocket.Emit("ReqGameEnd", paramstr);
    }

    // 请求结算
    public void CheckOutGame() {

        // 本地直接结算
        if (GameType != 2) {
            CurrentPlayer.Ins.winner_id = GetWinnerId();
            GameOver();
            return;
        }

        // 在线对战要通知服务器告知对手确认结算
        string paramstr = JsonConvert.SerializeObject(param);
        SocketIO.Ins.sdgSocket.Emit("ReqCheckOut", paramstr);
    }

    // 通知服务器确认结算
    public void ConfirmCheckOut() {
        ParamGameEnd param = new ParamGameEnd();
        param.userid = int.Parse(CurrentPlayer.Ins.user.userid);
        param.token = CurrentPlayer.Ins.user.token;
        // 胜利者
        param.winnerid = int.Parse(GetWinnerId());
        param.type = 0;// 正常胜利 
        string paramstr = JsonConvert.SerializeObject(param);
        SocketIO.Ins.sdgSocket.Emit("ReqGameEnd", paramstr);
        CloseDialog();
    }

    // 关闭对话框
    public void CloseDialog() {
        gameui.checkoutDialog.SetActive(false);
    }

    // 重新开始游戏
    public void ReStartGame() {
        // 初始化
        InitialData();
        // 在线对战初始化
        if (GameType == 2)
        {
            OnlineInit();
        }
        // 显示游戏界面UI
        gameui.object_gameui.SetActive(true);
        // 隐藏游戏结束界面
        gameendui.object_gameendui.SetActive(false);
        // 清空棋盘
        GoUIManager.Ins.ClearAllMoves();
        gameui.text_GameResult.text = "白棋领先"+game.komi+"目";
    }

    // 继续游戏
    public void ContinueGame() {
        // 在线对战不可续战
        if (GameType == 2) return;
        StartGame();
        // 显示游戏界面UI
        gameui.object_gameui.SetActive(true);
        // 隐藏游戏结束界面
        gameendui.object_gameendui.SetActive(false);
    }

    #endregion

    #region 落子操作
    // 人人对战
    bool SetMove_H_H(Point index)
    {
        if (game.SetMove(index, game.player))
        {
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
        // 玩家落子成功后电脑下棋
        if (game.player == huamnplayer && game.SetMove(index, huamnplayer))
        {
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
        genm = game.PointCorrect(genm);
        game.GoPanel[genm.x, genm.y].player = -1;
        Debug.Log("AI 落子生成："+genm.x + " "+genm.y);
        // 界面落子
        GoUIManager.Ins.setMove(genm, computerplayer);
        if (game.player == computerplayer && game.SetMove(genm, computerplayer))
        {
            // 落子成功
            PlayerChange();
        }
        else {
            // 失败撤销
            GoUIManager.Ins.deleteMove(genm);
            PlayerChange();
            Debug.Log("GNUGo AI 下棋失败！");
        }
        yield return 0;
    }

    // 不同模式落子
    public bool SetTypedMove(Point mouseIndex)
    {
        // 非游戏状态
        if (game.gameState != 1) {
            Debug.Log("游戏未开始！");
            return false;
        }
        bool success = false;

        // 鼠标坐标转换到0-1空间
        switch (game.gameType)
        {
            case 0:
                success = SetMove_H_H(mouseIndex);
                break;
            case 1:
                // 玩家先下棋
                if (game.player == 1) {
                    success = SetMove_H_C(mouseIndex);
                    // 玩家下棋成功后AI下棋
                    if (success) StartCoroutine(GNUComputerMove());
                }
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
        gameui.text_roomid.text = "房间号：" + CurrentPlayer.Ins.roomId.ToString();
        // 监听
        AddSocketIOListener();
    }
    
    // 初始化
    void InitialData()
    {
        // 初始化游戏
        int scale = GoUIManager.Ins.panelScale;
        game = new Game(GameType, scale);
        // 初始化当前棋子颜色
        game.player = 1;

        // 注册计时事件
        //timer = new Timer(game.moveTime);
        //timer.tickEvent += OnTimeEnd;
        //timer.tickSeceondEvent += OnSecond;
        //timerLabel.text = timer._currentTime.ToString();

        // 离线游戏直接开始
        if (GameType != 2) StartGame();

        // H-H
        if (GameType == 0) {
            CurrentPlayer.Ins.opponent.username = "友情对手";
        }

        // H-C
        if (GameType == 1) {
            CurrentPlayer.Ins.opponent.username = "GNUGo AI";
            CurrentPlayer.Ins.user.color = 1;
            CurrentPlayer.Ins.opponent.color = 0;
        }

        // Online
        if (GameType == 2) {
            // 客人执黑
            if (CurrentPlayer.Ins.isRoomOwner)
            {
                CurrentPlayer.Ins.user.color = 0;
                CurrentPlayer.Ins.opponent.color = 1;
                if (CurrentPlayer.Ins.player_num < 2)
                    CurrentPlayer.Ins.opponent.username = "";
            }
            else
            {
                CurrentPlayer.Ins.user.color = 1;
                CurrentPlayer.Ins.opponent.color = 0;
                if (CurrentPlayer.Ins.player_num < 2)
                    CurrentPlayer.Ins.opponent.username = "";
            }
        }

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
        gameui.text_GameState.text = "游戏中...";
        gameui.object_startgame.SetActive(false);
        //timer.StartTimer();
    }

    // 玩家切换
    public void PlayerChange()
    {
        // 下棋指示切换
        gameui.colorsToMove[game.player].SetActive(false);
        gameui.colorsToMove[game.PlayerToogle()].SetActive(true);

        // 重置计时器
        //timer.ResetTimer();
        //timer.StartTimer();
        //timerLabel.text = timer._currentTime.ToString();

        UpdateSore();

        game.ChangePlayer();
    }
     
    // 获胜者
    string GetWinnerId() {
        float score = Game.SDGGetScore();
        if ((score > 0.1 && (CurrentPlayer.Ins.user.color == 0)) || (score < -0.1 && CurrentPlayer.Ins.user.color == 1))
        {
            return CurrentPlayer.Ins.user.userid;
        }
        else if (score >= -0.1 && score <= 0.1)
        {
            return "-1";// 平局
        }
        else
        {
            return CurrentPlayer.Ins.opponent.userid;
        }
    }
    // 显示获胜者信息
    void ShowWinnerInfo()
    {
        // 隐藏游戏界面UI
        gameui.object_gameui.SetActive(false);
        // 显示游戏结束界面
        gameendui.object_gameendui.SetActive(true);
        gameendui.object_gameendicon[0].SetActive(false);
        gameendui.object_gameendicon[1].SetActive(false);

        if (CurrentPlayer.Ins.winner_id == "-1")
        {
            gameendui.text_gameendresult.text = "平局";
            return;
        }
        
        if (GameType == 0 || CurrentPlayer.Ins.winner_id == CurrentPlayer.Ins.user.userid)
        {
            // 胜利
            gameendui.object_gameendicon[0].SetActive(true);
        }
        else {
            // 失败
            gameendui.object_gameendicon[1].SetActive(true);
        }
        User winner = (CurrentPlayer.Ins.winner_id == CurrentPlayer.Ins.user.userid) ? CurrentPlayer.Ins.user : CurrentPlayer.Ins.opponent;
        string winner_color = winner.color == 1 ? "（黑方）" : "（白方）";
        string winner_name = (GameType == 0) ? "" : winner.username;
        gameendui.text_gameendresult.text = winner_name +winner_color + "胜";
    }

    // 更新成绩
    void UpdateSore() {
        // 最新成绩结算
        float wscore = Game.SDGGetScore();
        float bscore = -wscore;
        string wscorestr = string.Format("{0:F1}", wscore);
        string bscorestr = string.Format("{0:F1}",bscore);
        if (wscore > 0)
        {
            gameui.text_GameResult.text = "白棋领先" + wscorestr + "目！";
        }
        else
        {
            gameui.text_GameResult.text = "黑棋领先" + bscorestr + "目！";
        }
    }
    #endregion

    #region 计时器逻辑
    /*
    void OnTimeEnd()
    {
         Debug.Log("时间到！");
        //timer.EndTimer();
    }

    void OnSecond()
    {
        Debug.Log("又一秒！");
        //game.timeUsed++;
        //timerLabel.text = timer._currentTime.ToString();
    }
    */
    #endregion

}