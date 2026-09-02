Shader "Unlit/TextureColor"
{
    Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Brightness("Brightness", Range(0, 1)) = 0.5
		_Contrast("Contrast", Range(0, 4)) = 1.0
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
			float3 _Color;
			float _Brightness;
			float _Contrast;

			float3 ContrastFilter(float3 Color, float Brightness, float Contrast)
			{
				float3 pixelColor = Color;

				// Apply contrast.
				pixelColor.rgb = ((pixelColor.rgb - 0.5f) * max(Contrast, 0)) + 0.5f;

				// Apply brightness.
				pixelColor.rgb += Brightness;

				return pixelColor;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				float3 tex = tex2D(_MainTex, i.uv).rgb;

				//rgb = tex + _Color * tex;
				float3 rgb = ContrastFilter(tex + _Color * tex, _Brightness * 1.0, _Contrast * 2.0);
                return fixed4(rgb, 1.0) * _Brightness;
            }
            ENDCG
        }
    }
}
