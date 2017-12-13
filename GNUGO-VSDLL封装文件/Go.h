
#if defined (EXPORTBUILD)  
# define _DLLExport __declspec (dllexport)  
# else  
# define _DLLExport __declspec (dllimport)  
#endif  

extern "C"{
#include "gnugo.h"
#include "liberty.h"

#include "gg-getopt.h"
#include "gg_utils.h"
#include "winsocket.h"

#include "sgftree.h"
#include "random.h"
}
extern "C" int   _DLLExport Add(int x,int y);

extern "C" void  _DLLExport SDGGoInit(int sdg_boardsize);              // 初始化GNUGo
extern "C" int   _DLLExport SDGBoardStat(int i, int j);                // 棋盘棋子状态 0:白子 1:黑子 -1:无子
extern "C" float _DLLExport SDGGetScore();                             // 获取当前计分
extern "C" int   _DLLExport SDGIsAllowedMove(int i, int j, int color); // 落子合法性
extern "C" int   _DLLExport SDGPlayMove(int i, int j, int color);      // 指定坐标落子
extern "C" int   _DLLExport SDGUndoMove(int n);                        // 悔棋n步
extern "C" int   _DLLExport SDGGenComputerMove(int color);             // AI自动生成当前最优落子一维坐标