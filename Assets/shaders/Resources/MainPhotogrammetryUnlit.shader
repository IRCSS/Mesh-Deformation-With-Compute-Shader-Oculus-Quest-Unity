Shader "Unlit/MainPhotogrammetryUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

   

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

			struct _Vertex {
				float3 position;
				float3 velocity;
				float2 uv;
			};


			StructuredBuffer<_Vertex>  _VertexBuffer;
			float4x4  _MATRIX_M;
            sampler2D _MainTex;
            float4    _MainTex_ST;


			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
				uint   vid    : SV_VertexID;
			};

            v2f vert (appdata v)
            {
                v2f o;

				_Vertex vInfo  = _VertexBuffer[v.vid];

				float4 worldPos = mul(_MATRIX_M, float4(vInfo.position.xyz, 1.));

                o.vertex = UnityObjectToClipPos(worldPos);
                o.uv = vInfo.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
