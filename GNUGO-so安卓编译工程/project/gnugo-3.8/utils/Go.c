#include "gnugo.h"
#include "board.h"
#include "liberty.h"

#include "gg-getopt.h"
#include "gg_utils.h"
#include "winsocket.h"

#include "sgftree.h"
#include "random.h"

const int sdg_memory = 8; // gnugo占用内存上限
int resign = 0;
float value = 0;

// 0:白子 1：黑子
int getColor(int c){
	return c % 2 == 0 ? WHITE : BLACK;
}

// 初始化gnugo，sdg_boardsize为棋盘规模，如标准的：19（19x19）
void SDGGoInit(int sdg_boardsize)
{
	init_gnugo(8, 1);
	board_size = sdg_boardsize;
}

//0:白子 1 : 黑子 - 1 : 无子
int SDGBoardStat(int i, int j){
	int stat = -1;
	switch (BOARD(i, j))
	{
	case BLACK:
		stat = 1;
		break;
	case WHITE:
		stat = 0;
		break;
	default:
		stat = -1;
		break;
	}
	return stat;
}

// 获取当前棋盘的打分情况，正数白子领先，负数黑子领先
float SDGGetScore(){
	return gnugo_estimate_score(&white_score,&black_score);
}

// 落子合法性
int SDGIsAllowedMove(int i, int j, int color){
	int mycolor = getColor(color);
	if (is_allowed_move(POS(i, j), mycolor)){
		return 1;
	}
	else
	{
		return 0;
	}
}
// 在指定位置落下指定颜色的棋子（需要注意i,j坐标的转换，gnugo的棋盘原点在左上角，i坐标轴往下，j往右）
int SDGPlayMove(int i, int j, int color){
	int mycolor = getColor(color);
	gnugo_play_move(POS(i, j), mycolor);
	return 1;
}

// 悔棋n步
int SDGUndoMove(int n){
	return undo_move(n);
}

// GNUGo AI：当前棋盘，让gnugo计算产生颜色为color的棋子最优的落子位置，即让AI落子
int SDGGenComputerMove(int color) {
	return genmove(getColor(color), &value, &resign);
}

float SDGGetWhiteScore(){
	return white_score;
}

int SDGGetBoardSize(){
	return board_size;
}

int SDGGetLevel(){
	return get_level();
}
void SDGSetLevel(int new_level){
	set_level(new_level);
}
void SDGSetMaxLevel(new_max){
	set_max_level(new_max);
}
void SDGSetMinLevel(new_min){
	set_min_level(new_min);
}