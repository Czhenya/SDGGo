#define  EXPORTBUILD  
//加载头文件  
#include "Go.h"

const int sdg_memory = 8;
int resign = 0;
float value = 0;

colors getColor(int c){
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

float _DLLExport SDGGetScore(){
	return gnugo_estimate_score(&white_score,&black_score);
}

bool _DLLExport SDGPlayMove(int i, int j, int color){
	colors mycolor = getColor(color);
	if (is_legal(POS(i, j), color)){
		gnugo_play_move(POS(i, j), mycolor);
		return true;
	}
	return false;
}

int _DLLExport SDGGenComputerMove(int color) {
	
	return genmove(getColor(color), &value, &resign);

}