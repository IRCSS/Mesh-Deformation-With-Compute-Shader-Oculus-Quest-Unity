Shader "Unlit/shader_SkyClouds"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_ColorOne("ColorOne", Color) = (1,1,1,1)
		_ColorTwo("ColorTwo", Color) = (1,1,1,1)
    }
    SubShader
    {
	   Tags  {"Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 100ZWrite Off
			
		Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			float4 _ColorOne;
			float4 _ColorTwo;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

				float panning = _Time.y * 0.0085;

                // sample the texture
                float3 frontClouds = tex2D(_MainTex, float2(i.uv.x, -i.uv.y + 0.51)+ float2(panning,      0.));
			    float3 backClouds  = tex2D(_MainTex, float2(i.uv.x, -i.uv.y + 0.51)+ float2(panning*0.25, 0.));

				float3 col = frontClouds.rgb + backClouds.ggb;
				col *= _ColorOne;
				col = lerp(col, _ColorTwo, frontClouds.r);

		      	float alpha = smoothstep(0.5,1.0,i.uv.y);
                return float4(col.xyz, alpha)*alpha;
            }
            ENDCG
        }
    }
}
