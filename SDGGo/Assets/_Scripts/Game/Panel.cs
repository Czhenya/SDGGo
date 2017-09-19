using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SDG;

    [ExecuteInEditMode]
    public class Panel : Singleton<Panel>
    {
        // UI
        public Material mat;
        public Image stone;
        public Text[] playerText;
        public Text timerLabel;

        private Game game;                // 游戏对象
        private Timer timer;              // 计时器对象

        #region 脚本生命周期
        // 初始化
        void Start()
        {
            InitialData();
            InitialAllShaderData();
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
        if (game.SetMove(ref mat)) {
            game.PlayerChange();
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
    }