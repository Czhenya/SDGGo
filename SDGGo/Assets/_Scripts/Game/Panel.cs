using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using SDG;

[ExecuteInEditMode]
public class Panel : Singleton<Panel>
{
    // UI
    public int GameType;
    public Material mat;
    public Image stone;
    public Text[] playerText;
    public Text timerLabel;
    public Text ScoreLabel;
    public Text roomidLabel;
    public Text localplayerinfo;

    private Game game;                // 游戏对象
    private Timer timer;              // 计时器对象

    // 在线对战:
    // 本地棋手颜色
    int localPlayer = 1;
    // 对手落子位置
    Point oppenPos = new Point(0,0);
    // 对手是否已落子
    bool isOppenMoved = false;

    #region 脚本生命周期
    // 初始化
    void Start()
    {
        InitialData();
        InitialAllShaderData();
        StartGame();
    }

    // 帧回调
    void Update()
    {
        stone.color = game.player == 1 ? Color.black : Color.white;
        string localcolor = localPlayer == 1 ? "执黑" : "执白";
        localplayerinfo.text = "本地玩家信息：\n" + CurrentPlayer.Ins.user.username + "\n" + localcolor;

        // 对手落子
        if (isOppenMoved) {
            isOppenMoved = false;
            if (game.SetMove(oppenPos, game.player))
            {
                game.UpdateShader(ref mat);
                game.PlayerChange();
            }
        }
    }
    // 固定时间间隔回调
    void FixedUpdate()
    {
       // timer.UpdateTimer();
    }

    // 绘制shader材质
    void OnRenderImage(RenderTexture src, RenderTexture des)
    {
        Graphics.Blit(src, des, mat);
    }

    void OnDestroy()
    {
        // 注销计时事件
        //timer.tickEvent -= OnTimeEnd;
        //timer.tickSeceondEvent -= OnSecond;
    }

    #endregion

    #region 对外接口函数

    // 落子操作
    // 人人对战
    public void SetMove_H_H()
    {
        if (game.SetMove(game.player))
        {
            game.UpdateShader(ref mat);
            game.PlayerChange();
        }
    }
    // 人机对战
    void SetMove_H_C()
    {
        int huamnplayer = 1;
        if (game.player == huamnplayer && game.SetMove(huamnplayer))
        {
            game.UpdateShader(ref mat);
            game.PlayerChange();
        }
        else {
            return;
        }

        StartCoroutine(GNUComputerMove());
    }
    // 在线对战
    void SetMove_Online() {
        if (game.player == localPlayer) {
            if (game.SetMove(game.player))
            {
                game.UpdateShader(ref mat);
                game.PlayerChange();
            }
            // 通知服务器
            Point index = game.Position2Index(game.mousePosition);
            ParamPlayMove param = new ParamPlayMove();
            param.userid = int.Parse(CurrentPlayer.Ins.user.userid);
            param.token = CurrentPlayer.Ins.user.token;
            param.x = index.x;
            param.y = index.y;
            string paramstr = JsonConvert.SerializeObject(param);
            SocketIO.Ins.sdgSocket.Emit("ReqOperatePiece",paramstr);
        }
    }
    // 计算机落子
    IEnumerator GNUComputerMove() {
        yield return new WaitForSeconds(1);
        int computerplayer = 0;
        Point genm = game.GetGenComputerMove(computerplayer);
        game.mousePosition = game.Index2Position(genm);
        if (game.player == computerplayer && game.SetMove(genm, computerplayer))
        {
            game.UpdateShader(ref mat);
            game.PlayerChange();
        }
        yield return 0;
    }

    // 选子操作
    public void SelectMove(Vector2 mousePos)
    {
        // 鼠标坐标转换到0-1空间
        Vector2 curMousePos = new Vector2(mousePos.x / (float)Screen.width, mousePos.y / (float)Screen.height);
        Point curIndex = game.Position2Index(curMousePos);
        Point preIndex = game.Position2Index(game.mousePosition);
        game.mousePosition = curMousePos;
        if (curIndex.x == preIndex.x && curIndex.y == preIndex.y)
        {
            switch (game.gameType)
            {
                case 0:
                    SetMove_H_H();
                    break;
                case 1:
                    SetMove_H_C();
                    break;
                case 2:
                    SetMove_Online();
                    break;
                default:
                    break;
            }

            float wscore = Game.SDGGetScore();
            if (wscore > 0) {
                ScoreLabel.text = "白棋领先" + wscore + "点！";
            }
            else {
                float bscore = -wscore;
                ScoreLabel.text = "黑棋领先" + bscore + "点！";
            }
        }
        else
        {
            game.mousePosition = curMousePos;
            game.UpdateShader(ref mat);
        }
    }
    #endregion

    #region 自定义初始化函数
    // 在线对战初始化
    void OnlineInit() {
        // 房主先手
        if (CurrentPlayer.Ins.isRoomOwner)
        {
            game.Players[0] = new Player(CurrentPlayer.Ins.name, 1); // 房主执黑
            localPlayer = 1;
        }
        else {
            game.Players[1] = new Player(CurrentPlayer.Ins.name,0);  // 客人执白
            localPlayer = 0;
        }
        roomidLabel.text = "房间号："+CurrentPlayer.Ins.roomId.ToString();

        // 监听对手落子
        SocketIO.Ins.sdgSocket.On("RetOperatePiece",(data)=> {
            lock (oppenPos) {
                oppenPos = new Point(0,0);
                isOppenMoved = true;
            }
        });
    }
    // 初始化
    void InitialData()
    {
        // 初始化游戏
        int scale = mat.GetInt("_panelScale");
        float borderW = mat.GetFloat("_borderWidth");
        game = new Game(GameType,scale, borderW);
        // 在线对战
        if (GameType == 2) OnlineInit();
        // 初始化先手棋子颜色
        game.player = 1;

        // 注册计时事件
       /* timer = new Timer(game.moveTime);
        timer.tickEvent += OnTimeEnd;
        timer.tickSeceondEvent += OnSecond;
        timerLabel.text = timer._currentTime.ToString();*/
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

    // 游戏开始
    void StartGame() {
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

        timer.ResetTimer();
        timer.StartTimer();
        timerLabel.text = timer._currentTime.ToString();
    }
    #endregion


    #region 计时逻辑
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