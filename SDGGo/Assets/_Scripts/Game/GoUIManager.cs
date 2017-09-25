using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SDG;

public class GoUIManager : Singleton<GoUIManager> {

    public GameObject whiteStone;
    public GameObject blackStone;
    public Text coortext;
    // 四个角
    public Transform LTCorner, RTCorner, LBCorner, RBCorner;

    public int panelScale = 19;
    public float panelBorder = 0.1f;   // 棋盘边界
    public float panelWidth;           // 棋盘宽度
    float gapWidth;                    // 格子宽度
    GameObject[,] stones = new GameObject[19,19];

    void Start () {
        panelInit();
    }
	
	void Update () {
        if (!EventSystem.current.IsPointerOverGameObject()&&Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 hitpos = hit.point;//得到碰撞点的坐标
                Point mouseIndex = Pos2Index(hitpos);
                coortext.text = mouseIndex.x + "," + mouseIndex.y;
                setMove(Pos2PanelPos(hitpos), Panel.Ins.game.player);
                StartCoroutine(SetGNUMove(mouseIndex));
            }
        }
    }
    public void BackHome()
    {
        SceneManager.LoadScene("Menu");
    }

    void panelInit()
    {
        panelScale = 19;
        panelBorder = 0.1f;
        panelWidth = RTCorner.position.x - LTCorner.position.x;
        gapWidth = panelWidth / (panelScale - 1);
    }


    // 是否在棋盘内
    public bool isInPanel(Vector2 mouseops) {
       // if (p.x < LTCorner.position.x - panelBorder || p.x > RTCorner.position.x + panelBorder || p.y < RBCorner.position.y - panelBorder || p.y >RTCorner.position.y + panelBorder)
         //   return false;
        return true;
    }

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

    IEnumerator SetGNUMove(Point index) {
        yield return new WaitForSeconds(0.1f);
        Panel.Ins.SelectMove(index);
        yield return 0;
    }

    public void setMove(Point index, int color) {
        setMove(Index2PanelPos(index),color);
    }
    public void setMove(Vector2 mousePos, int color) {
        Vector3 stonePos = new Vector3(mousePos.x,mousePos.y,0);
        GameObject stoneColor = color == 1 ? blackStone : whiteStone;
        Point index = Pos2Index(mousePos);
        stones[index.x, index.y] = Instantiate(stoneColor, stonePos, stoneColor.transform.rotation);
    }

    public void deleteMove(Point index) {
        stones[index.x, index.y].SetActive(false);
    }
}
