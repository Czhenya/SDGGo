using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SDG;

public class GoUIManager : Singleton<GoUIManager> {

    public GameObject whiteStone;           // 白子
    public GameObject blackStone;           // 黑子
    //public GameObject stoneRing;          // 指示环
    public GameObject confirm;              // 确认落子按钮
    public LineRenderer line_vertical;      // 指示线
    public LineRenderer line_horizontal;

    // 四个角
    public Transform LTCorner, RTCorner, LBCorner, RBCorner;
    public int panelScale = 19;
    public float panelBorder = 0.1f;   // 棋盘边界
    public float panelWidth;           // 棋盘宽度
    float gapWidth;                    // 格子宽度

    // 棋子容器
    GameObject[,] stones = new GameObject[19,19];
    // 鼠标点击坐标记忆
    Point preMouseIndex = new Point(-1,-1);
    Point curMouseIndex = new Point(-1, -1);
    Vector3 hitpos = Vector3.zero;

    void Start () {
        panelInit();
    }
	
	void Update () {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                //得到碰撞点的坐标
                hitpos = hit.point;
                // 选中的坐标
                SelectMove(Pos2Index(hitpos));

                if (preMouseIndex.x == curMouseIndex.x && preMouseIndex.y == curMouseIndex.y)
                {
                    ConfirmMove();
                }
                else {
                    preMouseIndex = curMouseIndex;
                }
            }
        }

        // 键盘十字键
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
            SelectLeft();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            SelectRight();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            SelectDown();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            SelectUp();
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter)||Input.GetKeyDown(KeyCode.Space)) {
            ConfirmMove();
        }
    }

    // 棋盘初始化
    void panelInit()
    {
        panelScale = 19;
        panelBorder = 0.1f;
        panelWidth = RTCorner.position.x - LTCorner.position.x;
        gapWidth = panelWidth / (panelScale - 1);
        //stoneRing.SetActive(false);
    }

    // 上下左右移动
    public void SelectLeft() {
        if (curMouseIndex.x - 1 < 0) return;
        Point left = new Point(curMouseIndex.x - 1,curMouseIndex.y);
        SelectMove(left);
    }
    public void SelectRight()
    {
        if (curMouseIndex.x + 1 >= Panel.Ins.game.panelScale) return;
        Point right = new Point(curMouseIndex.x + 1, curMouseIndex.y);
        SelectMove(right);
    }
    public void SelectUp()
    {
        if (curMouseIndex.y + 1 >= Panel.Ins.game.panelScale) return;
        Point up = new Point(curMouseIndex.x, curMouseIndex.y+1);
        SelectMove(up);
    }
    public void SelectDown()
    {
        if (curMouseIndex.y - 1 < 0) return;
        Point down = new Point(curMouseIndex.x, curMouseIndex.y - 1);
        SelectMove(down);
    }

    // 返回主菜单
    public void BackHome()
    {
        Panel.Ins.GiveUpGame();
        GoSceneManager.Ins.EnterLoadingScene("Menu");
    }

#region 坐标转换
    // 整型坐标->棋盘精确坐标
    public Vector2 Index2PanelPos(Point index)
    {
        Vector2 panelPos = new Vector2(LBCorner.position.x + gapWidth*index.x,LBCorner.position.y + gapWidth * index.y);
        return panelPos;
    }
    // 精确坐标->棋盘整型坐标
    public Point Pos2Index(Vector2 mousePos)
    {
        Vector2 localPos = mousePos + new Vector2(panelWidth/2,panelWidth/2);
        int x = (int)Mathf.Round(((localPos.x) / gapWidth));
        int y = (int)Mathf.Round((localPos.y) / gapWidth);

        if (x < 0) x = 0;
        if (x >= panelScale) x = panelScale - 1;

        if (y < 0) y = 0;
        if (y >= panelScale) y = panelScale - 1;

        return new Point(x, y);
    }
    // 精确坐标->棋盘精确坐标
    public Vector2 Pos2PanelPos(Vector2 pos) {
        return Index2PanelPos(Pos2Index(pos));
    }
    #endregion

    #region 落子接口

    // 界面选中位置
    void SelectMove(Point curIndex)
    {
        curMouseIndex = curIndex;
        // 显示选中位置光标
        setRing(curIndex);
        // 显示确认落子按钮
        if(Panel.Ins.game.gameState == 1) confirm.SetActive(true);
    }

    // 确认落子
    public void ConfirmMove()
    {
        // 只能落在无子位置
        if (Panel.Ins.game.GetPanelColor(curMouseIndex) != -1) return;
        // 界面落子
        setMove(curMouseIndex, Panel.Ins.game.color);
        // 逻辑落子
        setLogicMove();
    }

    // 界面落子
    public void setMove(Point index, int color) {
        setMove(Index2PanelPos(index),color);
    }
    public void setMove(Vector2 pos, int color) {
        // 移除原棋子
        Point index = Pos2Index(pos);
        deleteMove(index);
        if (color == -1) return;

        // 落子位置，注意z坐标要和panel一致
        Vector3 stonePos = new Vector3(pos.x,pos.y,1);
        GameObject stoneColor = color == 1 ? blackStone : whiteStone;
        stones[index.x, index.y] = Instantiate(stoneColor, stonePos, stoneColor.transform.rotation);
    }
    // 逻辑落子
    public void setLogicMove() {
        StartCoroutine(SetGNUMove(curMouseIndex));
    }
    IEnumerator SetGNUMove(Point index)
    {
        yield return new WaitForSeconds(0.1f);
        if (Panel.Ins.SetTypedMove(index))
        {
            // 切换玩家
            Panel.Ins.PlayerChange();
            // 隐藏确认落子按钮
            confirm.SetActive(false);
        }
        // 刷新棋盘
        UpdateAllMoves();
        yield return 0;
    }

    // 移除界面指定棋子
    public void deleteMove(Point index) {
        if (stones[index.x, index.y]) {
            stones[index.x, index.y].SetActive(false);
            Destroy(stones[index.x, index.y]);
        }
    }
    // 清空棋盘
    public void ClearAllMoves() {
        for (int i = 0; i < panelScale; ++i) {
            for (int j = 0; j < panelScale; ++j) {
                deleteMove(new Point(i,j));
            }
        }
    }
    // 刷新棋盘
    public void UpdateAllMoves() {
        for (int i = 0; i < panelScale; ++i)
        {
            for (int j = 0; j < panelScale; ++j)
            {
                Point index = new Point(i, j);
                int color = Panel.Ins.game.GetPanelColor(index);
                setMove(index,color);
            }
        }
    }
#endregion

    // 设置指示环
    public void setRing(Point index) {
        setRing(Index2PanelPos(index));
    }
    public void setRing(Vector3 pos) {
        //stoneRing.transform.position = pos;
        float line_ver_x = pos.x;
        float line_hor_y = pos.y;

        float line_ver_y_start = LBCorner.position.y;
        float line_ver_y_end = LTCorner.position.y;

        float line_hor_x_start = LTCorner.position.x;
        float line_hor_x_end = RTCorner.position.x;

        float line_z = 0.5f; // line render的z坐标，注意要比panel的z坐标略小

        line_horizontal.SetPosition(0, new Vector3(line_hor_x_start, line_hor_y, line_z));
        line_horizontal.SetPosition(1, new Vector3(line_hor_x_end, line_hor_y, line_z));

        line_vertical.SetPosition(0, new Vector3(line_ver_x, line_ver_y_start, line_z));
        line_vertical.SetPosition(1, new Vector3(line_ver_x, line_ver_y_end, line_z));
    }
}
