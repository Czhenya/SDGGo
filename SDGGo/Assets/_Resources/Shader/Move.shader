Shader "Custom/Move" {
	 Properties 
    {
        _MainTint ("Diffuse Tint", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _SpecularColor ("Specular Color", Color) = (1,1,1,1)

        //高光强度
        _SpecPower ("Specular Power", Range(0.1,30)) = 1
        _SpecHardness ("Specular Hardness", Range(0.1, 10)) = 2

    }

    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        //自定义Phong光照
        #pragma surface surf Phong

        //添加对应的属性变量
        float4 _SpecularColor;
        sampler2D _MainTex;
        float4 _MainTint;
        float _SpecPower;
        float _SpecHardness;

        //实现光照函数,必要的参数可以参考Lighting.cginc
        //除了输出结构，这里需要有光照方向，视点方向，衰减系数
        //这里我们要创建的是高光着色器，所以我们需要加上视点
        inline fixed4 LightingPhong (SurfaceOutput s, fixed3 lightDir, half3 viewDir, fixed atten)
        {
            //计算漫反射矢量,声明漫反射组件
            //做顶点法线和光源方向的点积运算，将返回（-1,1）
            //当返回1时，表明物体整对着光源，反之同理
            float diff = dot(s.Normal, lightDir);

            //计算反射向量，实现法线朝向光源弯曲的效果
            float3 reflectionVector = normalize((2.0 * s.Normal * diff) - lightDir);

            //计算最终的高光
            float spec = pow(max(0,dot(reflectionVector, viewDir)), _SpecPower);
            float3 finalSpec = _SpecularColor.rgb * spec;

            //得到最后的颜色值
            fixed4 c;
            c.rgb = (s.Albedo * _LightColor0.rgb * diff) + (_LightColor0.rgb * finalSpec);
            c.a = 1.0;
            return c;
        }

        struct Input 
        {
            float2 uv_MainTex;
        };

        void surf (Input IN, inout SurfaceOutput o) 
        {
            half4 c = tex2D (_MainTex, IN.uv_MainTex) * _MainTint;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    } 
    FallBack "Diffuse"
}
