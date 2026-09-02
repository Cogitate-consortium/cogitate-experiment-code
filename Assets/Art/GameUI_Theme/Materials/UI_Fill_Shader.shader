// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurableRendering.shader" company="Supyrb">
//   Copyright (c) 2018 Supyrb. All rights reserved.
// </copyright>
// <repository>
//   https://github.com/supyrb/ConfigurableShaders
// </repository>
// <author>
//   Johannes Deml
//   send@johannesdeml.com
// </author>
// <documentation>
//   Wiki: https://github.com/supyrb/ConfigurableShaders/wiki/Blending
//   Blending examples: https://wiki.popcornfx.com/index.php/Particle_tutorial_smoke#Setting_up_the_billboard_renderer
// </documentation>
// --------------------------------------------------------------------------------------------------------------------

Shader "UI/Fill"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_FillAmountX("Fill Amount X", Range(0,1)) = 0.5
		_FillAmountY("Fill Amount Y", Range(0,1)) = 0.5

		[Header(Blending)]
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc("Blend mode Source", Int) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst("Blend mode Destination", Int) = 10

		[Header(Stencil)]
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		[Header(Rendering)]
		_ColorMask("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

		CGINCLUDE
#include "UnityCG.cginc"
#include "UnityUI.cginc"

		fixed4 _Color;
	fixed4 _TextureSampleAdd;
	float4 _ClipRect;
	float _FillAmountX, _FillAmountY;

	struct appdata_t
	{
		float4 vertex    : POSITION;
		float4 color    : COLOR;
		float2 texcoord : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f
	{
		float4 vertex    : SV_POSITION;
		fixed4 color : COLOR;
		float2 texcoord     : TEXCOORD0;
		float4 worldPosition : TEXCOORD1;
		UNITY_VERTEX_OUTPUT_STEREO
	};

	v2f vert(appdata_t IN)
	{
		v2f OUT;
		UNITY_SETUP_INSTANCE_ID(IN);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
		OUT.worldPosition = IN.vertex;
		OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

		OUT.texcoord = IN.texcoord;

		OUT.color = IN.color * _Color;
		return OUT;
	}

	sampler2D _MainTex;

	fixed4 frag(v2f IN) : SV_Target
	{
		half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

		color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

#ifdef UNITY_UI_ALPHACLIP
		clip(color.a - 0.001);
#endif
		
		color.a = (color.a > 0) && (IN.texcoord.x < _FillAmountX && IN.texcoord.y < _FillAmountY) ? 1.0 : 0.0;

		return color;
	}
		ENDCG

		SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

			Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}
			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend[_BlendSrc][_BlendDst]
			ColorMask[_ColorMask]

			Pass
		{

			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile __ UNITY_UI_ALPHACLIP
			ENDCG
		}
	}
}
