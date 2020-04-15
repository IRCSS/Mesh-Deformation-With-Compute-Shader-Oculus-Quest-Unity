// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/SkyBox"
{
	Properties
	{
		_TopColor    ("Top color"    , Color) = (1, 1, 1, 1)
		_HorizonColor("Horizon color", Color) = (1, 1, 1, 1)
		_SunVector   ("Sun vector"   , Vector)= (1, 1, 1, 1)
		_SunDecay    ("Sun Decay"    , Color) = (1, 1, 1, 1)
		_MieColor    ("MieColor"     , Color) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType" = "Background" "Queue" = "Background" }

		Pass
		{
		    ZWrite Off
			Cull   Off
			Fog { Mode Off }

			CGPROGRAM
	     	#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex   : POSITION;
				float3 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float3 texcoord : TEXCOORD0;
			};

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex   = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				return o;
			}
			
			float3 _TopColor;
			float3 _HorizonColor;
			float3 _SunVector;
			float3 _SunDecay;
			float3 _MieColor;

#define sunSize (100. + sin(_Time.x*20.)*10. +sin(_Time.x*40. +25.521)*5. )

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float3 dir = normalize(i.texcoord.xyz);
				float  f   = saturate(exp(-dir.y *1.5 ));
				float3 sunVector = (_SunVector);
				float sun  = exp(-dot(dir, sunVector)*sunSize - sunSize);
				_HorizonColor += saturate(exp(dot(dir.xz, -sunVector.xz))) * _MieColor ;

				fixed3 col = _TopColor * (1.0-f) + _HorizonColor *f + sun * _SunDecay;
				return col.xyzz;
			}
			ENDCG
		}
	}
}
