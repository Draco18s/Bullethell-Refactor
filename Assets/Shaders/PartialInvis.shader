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
				float4 f2 = ComputeGrabScreenPos(o.vertex);
				o.uv = float4(f1.x,f1.y,f2.x/f2.w,f2.y/f2.w);
				o.normal = v.normal;
				return o;
			}

			fixed mod(fixed x, fixed y) {
				return x - y * floor(x/y);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 render = fixed4(0,0,0,0);
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
				int indexoffset[25] =
				{
					 3, 89, 11, 29, 71, 7, 31, 61, 73, 2, 17, 41, 67, 37, 59, 13, 53, 19, 47, 79, 97, 23, 5, 43, 83
				};
				float scale = 1;

				int X = i.uv.x * (350/scale);
				int Y = 1 - i.uv.y * (350/scale);

				float tOff = indexoffset[(int)(_Time*30)%25]+_Time*30;
				int ind = ((Y + (350/scale)*(X + indexoffset[X%25])) - (int)tOff) % 51;

				while(ind < 0) ind += 51;

				int offset = fuzzoffset[ind] == -1 ? (fuzzoffset[ind-1] == -1 ? (fuzzoffset[ind-2] == -1 ? (fuzzoffset[ind-3] == -1 ? -3 : -2) : -1) : 0) : 1;
				
				float s = offset/_ScreenParams.y;

				fixed4 mainTex = tex2D(_MainTex, float2(i.uv.x, i.uv.y - s));
				float4 grabTex = tex2D(_GrabTexture, float2(i.uv.z, i.uv.w - s));

				offset = -(offset-2);
				float m = 1 +- (0.12*offset);

				render.rgb = grabTex * m;// + mainTex * (1-m);
				render.a = mainTex.a;

				#ifdef UNITY_UI_CLIP_RECT
				render.a *= UnityGet2DClipping(i.worldpos.xy, _ClipRect);
				#endif
				return render;
			}
		ENDCG
		}
	}
}