using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class SDGGoRuntime : MonoBehaviour {

    #region 运行时动态链接库

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

    // 测试
    [DllImport("sdggnugo")]
    public static extern int Add(int x, int y);

    // 初始化gnugo
    [DllImport("sdggnugo")]
    public static extern void SDGGoInit(int boardsize);

    // 棋盘棋子状态，0:白子 1:黑子 -1:无子
    [DllImport("sdggnugo")]
    public static extern int SDGBoardStat(int i, int j);

    // 获取当前打分，正数黑子领先，负数白子领先
    [DllImport("sdggnugo")]
    public static extern float SDGGetScore();

    // 判断GNUGo落子合法性
    [DllImport("sdggnugo")]
    public static extern int SDGIsAllowedMove(int i, int j, int color);
    
    // 在gnugo棋盘上指定位置落子
    [DllImport("sdggnugo")]
    public static extern int SDGPlayMove(int i, int j, int color);

    // gnugo落子一步并返回一维落子坐标，如果落子失败返回-1
    [DllImport("sdggnugo")]
    public static extern int SDGGenComputerMove(int color);

#elif UNITY_ANDROID
        
        // 初始化gnugo
        [DllImport("gnuGo-3.8")]
        public static extern void SDGGoInit(int boardsize);

        // 获取当前打分，正数黑子领先，负数白子领先
        [DllImport("gnuGo-3.8")]
        public static extern float SDGGetScore();

        // 在gnugo棋盘上指定位置落子，并返回是否落子成功
        [DllImport("gnuGo-3.8")]
        public static extern int SDGPlayMove(int i, int j, int color);

        // gnugo落子一步并返回一维落子坐标，如果落子失败返回-1
        [DllImport("gnuGo-3.8")]
        public static extern int SDGGenComputerMove(int color);

        // 判断GNUGo落子合法性
        [DllImport("gnuGo-3.8")]
        public static extern int SDGIsAllowedMove(int i, int j, int color);

        // 棋盘棋子状态，0:白子 1:黑子 -1:无子
        [DllImport("gnuGo-3.8")]
        public static extern int SDGBoardStat(int i, int j);
        
#endif
    #endregion
}
