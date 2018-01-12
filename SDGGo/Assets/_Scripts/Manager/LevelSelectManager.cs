using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour {

    /// <summary>
    /// 入门
    /// </summary>
    public void EnterRumenAI() {
        CurrentPlayer.Ins.curLevel = -10;
        SceneManager.LoadScene("GAME_H_C");
    }

    /// <summary>
    /// 初级
    /// </summary>
    public void EnterChujiAI()
    {
        CurrentPlayer.Ins.curLevel = 0;
        SceneManager.LoadScene("GAME_H_C");
    }

    /// <summary>
    /// 中级
    /// </summary>
    public void EnterZhongjiAI()
    {
        CurrentPlayer.Ins.curLevel = 5;
        SceneManager.LoadScene("GAME_H_C");
    }

    /// <summary>
    /// 高级
    /// </summary>
    public void EnterGaojiAI()
    {
        CurrentPlayer.Ins.curLevel = 10;
        SceneManager.LoadScene("GAME_H_C");
    }

    /// <summary>
    /// 困难
    /// </summary>
    public void EnterKunnanAI()
    {
        CurrentPlayer.Ins.curLevel = 15;
        SceneManager.LoadScene("GAME_H_C");
    }
}
