Shader "taecg/SkillIndicator/Circle" 
{
    Properties 
    {
		[Header(Base)]
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("MainTex", 2D) = "white" {}
		_Intensity("Intensity", float) = 1

		[Header(Sector)]
		[MaterialToggle] _Sector("Sector", Float) = 1
        _Angle ("Angle", Range(0, 360)) = 60
        _Outline ("Outline", Range(0, 5)) = 0.35
		_OutlineAlpha("Outline Alpha",Range(0,1))=0.5
		[MaterialToggle] _Indicator("Indicator", Float) = 1	//预警的圆形大范围底图

		[Header(Flow)]
		_FlowColor("Flow Color",color) = (1,1,1,1)
		_FlowFade("Fade",range(0,1)) = 1
		_Duration("Duration",range(0,1)) = 0

		[Header(Blend)]
		//混合方式
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
    }

    SubShader 
    {
        Pass 
        {
			Tags { "LightMode" = "ForwardBase" "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
			Blend [_SrcBlend][_DstBlend]
			ZWrite [_ZWrite]
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag  
            #include "UnityCG.cginc"
            #pragma target 2.5
			#pragma multi_compile __ _INDICATOR_ON

            fixed4 _Color;
            sampler2D _MainTex; uniform float4 _MainTex_ST;
			half _Intensity;
            float _Angle;
            fixed _Sector;
            fixed _Outline;
			fixed _OutlineAlpha;

			fixed4 _FlowColor;
			fixed _FlowFade;
			fixed _Duration;

            struct appdata 
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f 
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v) 
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f,o);

                o.uv = v.texcoord;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target 
            {
				fixed4 col = 0;
				fixed2 uv = i.uv;
				fixed4 mainTex = tex2D(_MainTex, uv);
				mainTex *= _Intensity;

#if _INDICATOR_ON
				return mainTex.b * 0.6 * _Color;
#endif

				//极坐标
                float2 centerUV = (uv * 2 - 1);
                float atan2UV = 1-abs(atan2(centerUV.g, centerUV.r)/3.14);

				//扇形切割
				fixed sector = lerp(1.0, 1.0 - ceil(atan2UV - _Angle*0.002777778), _Sector);
				//大一点的扇形做扇形两边的连线
				fixed sectorBig = lerp(1.0, 1.0 - ceil(atan2UV - (_Angle+ _Outline) * 0.002777778), _Sector);
				fixed outline = (sectorBig -sector) * mainTex.g * _OutlineAlpha;

				fixed needOutline = 1 - step(359, _Angle);
				outline *= needOutline;
				col = mainTex.r * _Color * sector + outline * _Color;

				//圆形的流光
				fixed flowCircleInner = smoothstep(_Duration - _FlowFade, _Duration, length(centerUV));	//渐变的内圈
				fixed flowCircleMask = step(length(centerUV), _Duration);	//硬边遮罩
				fixed4 flow = flowCircleInner * flowCircleMask * _FlowColor *mainTex.g * sector;

				col += flow;
                return col;
            }
            ENDCG
        }
    }
}
