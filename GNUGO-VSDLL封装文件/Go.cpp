#define  EXPORTBUILD  
//加载头文件  
#include "Go.h"

const int sdg_memory = 8;
int resign = 0;
float value = 0;

int getColor(int c){
	return c % 2 == 0 ? WHITE : BLACK;
}

int _DLLExport Add(int x, int y){
	return x + y;
}

void _DLLExport SDGGoInit(int sdg_boardsize)
{
	init_gnugo(8, 1);
	board_size = sdg_boardsize;
}

//0:白子 1 : 黑子 - 1 : 无子
int _DLLExport SDGBoardStat(int i, int j){
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

float _DLLExport SDGGetScore(){
	return gnugo_estimate_score(&white_score,&black_score);
}

int _DLLExport SDGIsAllowedMove(int i, int j, int color){
	int mycolor = getColor(color);
	if (is_allowed_move(POS(i, j), mycolor)){
		return 1;
	}
	else
	{
		return 0;
	}
}

int _DLLExport SDGPlayMove(int i, int j, int color){
	int mycolor = getColor(color);
	gnugo_play_move(POS(i, j), mycolor);
	return 1;
}

int _DLLExport SDGUndoMove(int n){
	return undo_move(n);
}

int _DLLExport SDGGenComputerMove(int color) {
	
	return genmove(getColor(color), &value, &resign);

}