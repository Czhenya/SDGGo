// DemoGo.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
using namespace std;

// 整型和GNUGo库颜色枚举的转换，否则传参对接会有问题
colors getColor(int c){
	return c % 2 == 0 ? WHITE : BLACK;
}

// 获取棋盘某个位置的状态
int boardstat(int i, int j){
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
		break;
	}
	return stat;
}

// 绘制棋盘状态
void displayboard(){
	for (int i = 0; i < board_size; i++)
	{ 
		for (int j = 0; j < board_size; j++)
		{
			cout << " " << boardstat(i, j);
		}
		cout << endl;
	}
}

// 落子测试
bool playmove(int i, int j, int c){
	colors mycolor = getColor(c);

	if (is_legal(POS(i, j), mycolor)){
		gnugo_play_move(POS(i, j), mycolor);
		showboard(0);
		//displayboard();
		float res =gnugo_estimate_score(&white_score,&black_score);
		cout << "score" << res << endl;
		return true;
	}
	return false;
}

// 清空棋盘
void ClearBoard(int boardsize){
	gnugo_clear_board(boardsize);
}

// 悔棋一步测试
int UndoMove(){
	return undo_move(1);
}

// 初始化
void GoInit(int boardSize){
	init_gnugo(8, 1);
	board_size = boardSize;
	showboard(0);
}

int _tmain(int argc, _TCHAR* argv[])
{
	GoInit(19);
	
	int i, j, c;
	while (std::cin >> i >> j)
	{
		// 悔棋
		if (i==-1)
		{
			if (UndoMove() == 1)
				showboard(0);
			continue;
		}

		if (playmove(i, j, 1)){
			showboard(0);
		}
		else{
			cout << "下棋失败！" << endl;
		}
		
		int resign = 0;
		float value = 0;
		int genm = genmove(WHITE, &value , &resign);
		cout << "gennerate:" << genm << endl;
		cout << "resign:" << resign << "  value:" << value << endl;

		if (playmove(I(genm), J(genm), 0)){
			showboard(0);
		}
		else{
			cout << "AI下棋失败！" << endl;
		}
	}

	return 0;
}