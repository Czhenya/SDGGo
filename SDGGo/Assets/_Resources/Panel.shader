// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Panel" {

	Properties
	{
		_backgroundColor("棋盘背景", Color) = (1.0, 1.0, 1.0, 1.0)
		_lineColor("棋盘线颜色", Color) = (1.0, 1.0, 1.0, 1.0)
		_starColor("棋星颜色", Color) = (1.0, 1.0, 1.0, 1.0)
		_blackColor("黑棋子颜色", Color) = (0, 0, 0, 0)
		_whiteColor("白棋子颜色", Color) = (1.0, 1.0, 1.0, 1.0)
		_stoneBorderColor("棋子边框颜色", Color) = (1.0, 0, 0, 0)

		_moveR("棋子半径", Range(0, 0.05)) = 0.02
		_starR("棋星半径", Range(0, 0.05)) = 0.01
		_lineWidth("棋盘线宽度", Range(0, 0.01)) = 0.005
		_borderWidth("棋盘边界宽度", Range(0, 0.5)) = 0.05
		_stoneBorderWidth("棋子边框宽度", Range(0, 0.01)) = 0.005

		_panelScale("棋盘规模", Int) = 19
	}

	SubShader{
			Cull Off
			ZWrite Off
			ZTest Always

			CGINCLUDE

			ENDCG

			Pass{

				CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

				uniform int _panelScale;

				uniform float4 _backgroundColor; // background color
				uniform float4 _lineColor;       // line color
				uniform float4 _starColor;
				uniform float4 _blackColor;
				uniform float4 _whiteColor;
				uniform float4 _stoneBorderColor;

				uniform float _moveR;
				uniform float _starR;
				uniform float _lineWidth;
				uniform float _borderWidth;
				uniform float _stoneBorderWidth;

				uniform float4 _MovesBlack[19*19];
				uniform float4 _MovesWhite[19*19];
				uniform float _worms[19 * 19];  // 形势
				uniform int _lastPlayer;
				uniform float _mousePosX;
				uniform float _mousePosY;
				uniform int _StepsBlack;
				uniform int _StepsWhite;

				uniform float4 _Stars[9];

				struct appdata
				{
					float4 vertex: POSITION;
					float4 uv: TEXCOORD0;
				};

				struct v2f
				{
					float2 uv:TEXCOORD0;
					float4 vertex:SV_POSITION;
				};

				// 顶点着色器函数
				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				// 片段着色器函数
				fixed4 frag(v2f i) :SV_Target
				{
					// 屏幕宽高比
					float aspectRatio = _ScreenParams.x / _ScreenParams.y;

					// 棋盘参数
					float borderW = _borderWidth;
					float panelWidth = 1 - 2 * borderW;
					float panelHeight = aspectRatio * panelWidth;
					float borderH = (1 - panelHeight) / 2;

					float gap_height = panelHeight / (_panelScale - 1);
					float gap_width = panelWidth / (_panelScale - 1);

					fixed4 finalColor = _backgroundColor;

					float2 pixelPos = float2(i.uv.x, i.uv.y);
					float2 pixelP = float2(pixelPos.x, pixelPos.y / aspectRatio);

					// 绘制棋盘线
					for (int j = 0; j < _panelScale; ++j) {
						float startY = borderH + j * gap_height;
						float startX = borderW + j * gap_width;
						if (((i.uv.y >= startY) && (i.uv.y <= startY + _lineWidth) && (i.uv.x >= borderW) && (i.uv.x <= borderW + panelWidth)) || ((i.uv.x >= startX) && (i.uv.x <= startX + _lineWidth) && (i.uv.y >= borderH) && (i.uv.y <= borderH + panelHeight))){
							finalColor = _lineColor;
						}
					}

					// 绘制星
					for (int s = 0; s < 9; ++s){
						float2 pos = float2(_Stars[s].x, _Stars[s].y / aspectRatio);
							if (length(pixelP - pos) <= _starR){
								finalColor = _starColor;
							}
					}

					// 绘制黑子
					for (int m = 0; m < _StepsBlack; ++m) {
						float2 pos = float2(_MovesBlack[m].x, _MovesBlack[m].y);
						pos = float2(pos.x, pos.y / aspectRatio);
						if (length(pos - pixelP) <= _moveR) {
							finalColor = _blackColor;
						}
					}
					
					// 绘制白子
					for (int n = 0; n < _StepsWhite; ++n) {
						float2 pos = float2(_MovesWhite[n].x, _MovesWhite[n].y);
							pos = float2(pos.x, pos.y / aspectRatio);
						if (length(pos - pixelP) <= _moveR) {
							finalColor = _whiteColor;
						}
					}
					// 最后一个棋子的边框
					float2 lastPos = float2(_mousePosX, _mousePosY / aspectRatio);//(_lastPlayer == 1) ? float2(_MovesWhite[_StepsWhite - 1].x, _MovesWhite[_StepsWhite - 1].y / aspectRatio) : float2(_MovesBlack[_StepsBlack - 1].x, _MovesBlack[_StepsBlack - 1].y / aspectRatio);
					bool topBottomCon = (pixelP.y > lastPos.y - _moveR) && (pixelP.y < lastPos.y + _moveR);
					bool leftRightCon = (pixelP.x > lastPos.x - _moveR) && (pixelP.x < lastPos.x + _moveR);
					bool left = pixelP.x < (lastPos.x - _moveR) && pixelP.x >(lastPos.x - _moveR - _stoneBorderWidth) && topBottomCon;
					bool right = pixelP.x >(lastPos.x + _moveR) && pixelP.x < (lastPos.x + _moveR + _stoneBorderWidth) && topBottomCon;
					bool top = pixelP.y >(lastPos.y + _moveR) && pixelP.y < (lastPos.y + _moveR + _stoneBorderWidth) && leftRightCon;
					bool bottom = pixelP.y < (lastPos.y - _moveR) && pixelP.y >(lastPos.y - _moveR - _stoneBorderWidth) && leftRightCon;
					if (left || right || top || bottom)
						finalColor = _stoneBorderColor;

					// 绘制形势
					//for (int x = 0; x < 1; ++x) {
					//	for (int y = 0; y < 1;++y){
							//if ((int)(_worms[x*_panelScale + y]) == -1) return finalColor;
							//float4 wormColor;
							//if ((int)(_worms[x*_panelScale + y]) == 0) wormColor = _whiteColor;
							//if ((int)(_worms[x*_panelScale + y]) == 1) wormColor = _blackColor;
						//	float pos = float2(borderW, borderH / aspectRatio);
						//	if (length(pos - pixelP) <= _moveR*2){
						//		finalColor = _blackColor;
						//	}
						//}
					//}

					return finalColor;
				}

					ENDCG
			}
		}
		FallBack "Diffuse"
}