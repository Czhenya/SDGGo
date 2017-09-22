using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SDG;

public class GoUIManager : Singleton<GoUIManager> {

    public GameObject whiteStone;
    public GameObject blackStone;

    public Transform LTCorner, RTCorner, LBCorner, RBColor;
    // 四个角屏幕坐标
    Vector3 LTPos, RTPos,LBPos,RBPos;

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
            mousePosition = Input.mousePosition;
            setMove(mousePosition,1);
        }
    }

    void panelInit() {
        panelScale = 19;
        // 获取四个角的屏幕坐标
        LTPos = Camera.main.WorldToScreenPoint(LTCorner.position);
        LTPos = Camera.main.WorldToScreenPoint(LTCorner.position);
        LTPos = Camera.main.WorldToScreenPoint(LTCorner.position);
        LTPos = Camera.main.WorldToScreenPoint(LTCorner.position);
        panelWidth = RTPos.x - LTPos.x;
        gapWidth = panelWidth / (panelScale-1);
    }

    // 是否在棋盘内
    bool isInPanel(Vector2 p) {
        if (p.x < LTPos.x-panelBorder || p.x > RTPos.x+panelBorder || p.y < RBPos.y-panelBorder || p.y > RTPos.y+panelBorder)
            return false;
        return true;
    }

    // 整型坐标->棋盘棋子精确坐标
    public Vector2 Index2PanelPos(Point index)
    {
        Point localIndex = new Point(index.x - panelScale/2, index.y - panelScale/2);
        return new Vector2(LBPos.x + gapWidth*index.x, LBPos.y + gapWidth * index.y);
    }
    // 精确坐标->棋盘整型坐标
    public Point Pos2Index(Vector2 mousePos)
    {
        int x = (int)Mathf.Round(((mousePos.x - LBPos.x) / gapWidth));
        int y = (int)Mathf.Round((mousePos.y - LBPos.y) / gapWidth);

        if (x < 0) x = 0;
        if (x >= panelScale) x = panelScale - 1;

        if (y < 0) y = 0;
        if (y >= panelScale) y = panelScale - 1;

        return new Point(x, y);
    }
    // 精确坐标->棋盘棋子精确坐标
    public Vector2 Pos2PnaelPos(Vector2 mousePos)
    {
        return Index2PanelPos(Pos2Index(mousePos));
    }

    void setMove(Point mousePos, int color) {
        Vector2 p2d = Index2PanelPos(mousePos);
        GameObject stone = color == 1 ? blackStone : whiteStone;
        Instantiate(stone, new Vector3(p2d.x, p2d.y, -10), stone.transform.rotation);
    }
    void setMove(Vector2 mousePos, int color) {
        setMove(Pos2Index(mousePos), color);
    }
}
