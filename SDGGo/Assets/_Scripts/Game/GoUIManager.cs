using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDG;

public class GoUIManager : Singleton<GoUIManager> {

    public GameObject whiteStone;
    public GameObject blackStone;
    public Transform GoPanel;
    // 四个角
    public Transform LTCorner, RTCorner, LBCorner, RBCorner;

    int panelScale;
    float panelBorder;               // 棋盘边界
    float panelWidth;                // 棋盘宽度
    float gapWidth;                  // 格子宽度
    Vector2 mousePosition;

    // Use this for initialization
    void Start () {
        panelInit();
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 hitpos = hit.point;//得到碰撞点的坐标
                setMove(TransformMousePos(hitpos), 1);
            }
        }
    }

    void panelInit() {
        panelScale = 19;
        panelBorder = 0.1f;
        panelWidth = RTCorner.position.x - LTCorner.position.x;
        gapWidth = panelWidth / (panelScale-1);
    }
    // 鼠标屏幕坐标->世界坐标
    Vector3 TransformMousePos(Vector3 mousepos) {
        // 鼠标坐标原点转换到棋盘坐下角
        Vector3 lbmousepos = new Vector3(mousepos.x+panelWidth/2, mousepos.y+panelWidth/2, mousepos.z);
        Debug.Log("落子坐标："+lbmousepos);
        return lbmousepos;
    }

    // 是否在棋盘内
    bool isInPanel(Vector2 p) {
        if (p.x < LTCorner.position.x - panelBorder || p.x > RTCorner.position.x + panelBorder || p.y < RBCorner.position.y - panelBorder || p.y >RTCorner.position.y + panelBorder)
            return false;
        return true;
    }

    // 整型坐标->棋盘棋子精确坐标
    public Vector3 Index2PanelPos(Point index)
    {
       // Point localIndex = new Point(index.x - panelScale/2, index.y - panelScale/2);
        return new Vector3(LBCorner.position.x + gapWidth*index.x,LBCorner.position.y + gapWidth * index.y,LBCorner.position.z);
    }
    // 精确坐标->棋盘整型坐标
    public Point Pos2Index(Vector3 mousePos)
    {
        int x = (int)Mathf.Round(((mousePos.x) / gapWidth));
        int y = (int)Mathf.Round((mousePos.y) / gapWidth);

        if (x < 0) x = 0;
        if (x >= panelScale) x = panelScale - 1;

        if (y < 0) y = 0;
        if (y >= panelScale) y = panelScale - 1;

        return new Point(x, y);
    }
    // 精确坐标->棋盘棋子精确坐标
    public Vector3 Pos2PnaelPos(Vector3 mousePos)
    {
        return Index2PanelPos(Pos2Index(mousePos));
    }

    void setMove(Point mousePos, int color) {
        Vector3 p2d = Index2PanelPos(mousePos);
        GameObject stone = color == 1 ? blackStone : whiteStone;
        Instantiate(stone, p2d, stone.transform.rotation);
    }
    void setMove(Vector3 mousePos, int color) {
        Point index = Pos2Index(mousePos);
        setMove(index,color);
    }
}
