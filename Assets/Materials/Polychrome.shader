Shader "Effects/Polychrome"
{
	Properties
	{
        _MainTex ("Texture", 2D) = "black" {}
        _Intensity ("Intensity", float) = 3.5
        _EphemeralStrength ("EphemeralStrength", float) = 1

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
				float2 uv  : TEXCOORD0;
                fixed2 normal : TEXCOORD1;
				float4 worldpos   : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _ClipRect;
			float4 _MainTex_ST;
			fixed _Intensity;
			fixed _EphemeralStrength;

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.worldpos = v.vertex;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = v.normal;
				return o;
			}

			fixed mod(fixed x, fixed y) {
				return x - y * floor(x/y);
			}

			fixed4 HSL(fixed4 c)
			{
				fixed low = min(c.r, min(c.g, c.b));
				fixed high = max(c.r, max(c.g, c.b));
				fixed delta = high - low;
				fixed sum = high+low;

				//delta *= delta > 0.03;
				//delta *= delta > 0.97;

				fixed4 hsl = fixed4(.0, .0, .5 * sum, c.a);
				if (delta == .0)
					return hsl;

				hsl.y = (hsl.z < .5) ? delta / sum : delta / (2.0 - sum);

				if (high == c.r)
					hsl.x = (c.g - c.b) / delta;
				else if (high == c.g)
					hsl.x = (c.b - c.r) / delta + 2.0;
				else
					hsl.x = (c.r - c.g) / delta + 4.0;

				hsl.x = mod(hsl.x / 6., 1.);
				return hsl;
			}

			fixed hue(fixed s, fixed t, fixed h)
			{
				fixed hs = mod(h, 1.)*6.;
				if (hs < 1.) return (t-s) * hs + s;
				if (hs < 3.) return t;
				if (hs < 4.) return (t-s) * (4.-hs) + s;
				return s;
			}

			fixed4 RGB(fixed4 c)
			{
				if (c.y < 0.0001)
					return fixed4(c.z, c.z, c.z, c.a);

				fixed t = (c.z < .5) ? c.y*c.z + c.z : -c.y*c.z + (c.y+c.z);
				fixed s = 2.0 * c.z - t;
				return fixed4(hue(s,t,c.x + 1./3.), hue(s,t,c.x), hue(s,t,c.x - 1./3.), c.w);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed _t = _Time * 2;
				fixed tilt_angle = _t * 1.9378237 + 3.7;
				fixed amt = 0.2*(0.5+cos(tilt_angle))*0.3;

				fixed polyX = 1 + (_t)/28.0 + amt;
				fixed polyY = _t;

				fixed time = 123.33412 * (_t / 1.14212) % 3000;
				
				float ephemeral = sin(_Time*15 + i.uv.y*20);
				float2 tuv = float2((i.uv.x + 0.03 * ephemeral * _EphemeralStrength)%1, i.uv.y);
				fixed4 tex = tex2D(_MainTex, tuv);
				float originalAlpha = tex.a;

				fixed2 uv = i.uv + i.normal + i.worldpos.x/200 / (_Intensity * 1.7);

				fixed low = min(tex.r, min(tex.g, tex.b));
				fixed high = max(tex.r, max(tex.g, tex.b));
				fixed delta = high - low;
				float bonusRed = 0.85 * (delta == 0);
				low = min(tex.r, min(tex.g, tex.b)-bonusRed);
				high = max(tex.r, max(tex.g, tex.b)-bonusRed);
				delta = high - low;
				fixed saturation_fac = 1. - max(0., 0.05 * (1.1-delta));

				fixed4 hsl = HSL(fixed4(tex.r * saturation_fac, (tex.g-bonusRed) * saturation_fac, tex.b, tex.a));
				
				fixed t = polyY * 2.221 + time;

				fixed2 texture_details = fixed2(71,95)*1000;
				fixed2 floored_uv = floor(uv*texture_details)/texture_details;
				fixed2 uv_scaled_centered = (floored_uv - 0.5) * 2.3 * max(texture_details.x, texture_details.y) * (_Intensity/10) / 1000;
	
				fixed2 field_part1 = uv_scaled_centered + 50.*fixed2(sin(-t / 143.6340), cos(-t / 99.4324));
				fixed2 field_part2 = uv_scaled_centered + 50.*fixed2(cos( t / 53.1532),  cos( t / 61.4532));
				fixed2 field_part3 = uv_scaled_centered + 50.*fixed2(sin(-t / 87.53218), sin(-t / 49.0000));

                fixed field = (1.+ (
					cos(length(field_part1) / 19.483) + sin(length(field_part2) / 33.155) * cos(field_part2.y / 15.73) +
					cos(length(field_part3) / 27.193) * sin(field_part3.x / 21.92) ))/2.;

				fixed res = (.5 + .5* cos( (polyX) * 2.612 + ( field + -.5 ) * 3.14));
				hsl.x = hsl.x+ res + polyY * 0.04;
				hsl.y = min(0.6,hsl.y+0.5);

				tex.rgb = RGB(hsl).rgb;
				float red = tex.r + 0.25;
				
				tex.r = tex.r * (1-_EphemeralStrength) + red * (_EphemeralStrength) * 0.6;
				tex.g = tex.g * (1-_EphemeralStrength) + red * (_EphemeralStrength) * 0.6 + tex.b * (_EphemeralStrength)*0.4;
				tex.b = tex.b * (1-_EphemeralStrength) + red * (_EphemeralStrength) * 0.6 + tex.b * (_EphemeralStrength)*0.4;

				#ifdef UNITY_UI_CLIP_RECT
				tex.a *= UnityGet2DClipping(i.worldpos.xy, _ClipRect);
				#endif
				//tex.a = _EphemeralStrength > 1.1 ? 1.4 : tex.a;
				return tex;
			}
		ENDCG
		}
	}
}