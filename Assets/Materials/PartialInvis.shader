// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Effects/PartialInvis"
{
	Properties
	{
        _MainTex ("Texture", 2D) = "black" {}

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

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

		GrabPass { }

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass
		{
			Name "Default"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			#pragma multi_compile __ UNITY_UI_ALPHACLIP


			struct appdata
			{
				float4 vertex   : POSITION;
				float2 uv : TEXCOORD0;
				fixed3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
                fixed2 normal : NORMAL;
				float4 uv  : TEXCOORD0;
				//float2 screenPos   : TEXCOORD2;
				float4 grabUV   : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			sampler2D _GrabTexture;
			float4 _ClipRect;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				float2 f1 = TRANSFORM_TEX(v.uv, _MainTex);
				float2 f2 = ComputeScreenPos(o.vertex);
				o.uv = float4(f1.x,f1.y,f2.x/(_ScreenParams.x/177),f2.y/(_ScreenParams.y/99.55));
				o.normal = v.normal;
				o.grabUV = ComputeGrabScreenPos(o.vertex);
				return o;
			}

			fixed mod(fixed x, fixed y) {
				return x - y * floor(x/y);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				int fuzzoffset[51] =
				{
					1,-1,1,-1,1,1,-1,
					1,1,-1,1,1,1,-1,
					1,1,1,-1,-1,-1,-1,
					1,-1,-1,1,1,1,1,-1,
					1,-1,1,1,-1,-1,1,
					1,-1,-1,-1,-1,1,1,
					1,1,-1,1,1,-1,1,1 
				};
				float scale = 1;

				int X = i.uv.x * (250/scale);
				int Y = i.uv.w * (350/scale);
				int ind = ((Y + (350/scale)*X) /*+ (_Time*300)*/) % 51;

				int offset = fuzzoffset[ind] == -1 ? (fuzzoffset[ind-1] == -1 ? (fuzzoffset[ind-2] == -1 ? (fuzzoffset[ind-3] == -1 ? -4 : -3) : -2) : -1) : 1;
				
				fixed4 tex = tex2D(_MainTex, float2(i.uv.x, i.uv.y - (offset/_ScreenParams.y*0.2)));
				float4 p = tex2D(_GrabTexture, float2(i.uv.z, i.uv.w - (offset/_ScreenParams.y*1)));
				float m = fuzzoffset[ind] == -1 ? (fuzzoffset[ind-1] == -1 ? (fuzzoffset[ind-2] == -1 ? (fuzzoffset[ind-3] == -1 ? .9 : .81) : .73) : .65) : 1;

				tex.rgb = tex.rgb * (1-m) + p * (m);

				#ifdef UNITY_UI_CLIP_RECT
				tex.a *= UnityGet2DClipping(i.worldpos.xy, _ClipRect);
				#endif
				return tex;
			}
		ENDCG
		}
	}
}