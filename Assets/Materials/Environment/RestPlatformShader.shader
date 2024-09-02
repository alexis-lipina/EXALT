// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// for rest platforms, which can be charged and need to respond visibly to that. I know its ad hoc as hell to do this but lemme just finish this project pleeeeease dont judge
Shader "Custom/RestPlatformShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_GlowRampTex("Glow Value Gradient", 2D) = "white" {}
		_GlowColorMapTex("Glow Color Map", 2D) = "white" {}
		_GlowOpacity("Glow Opacity", Float) = 1.0
		_GlowUpperClip("Glow Upper Clip Value", Float) = 1.0 // these values are used to actually change the glow over time and show the "meter" building
		_GlowLowerClip("Glow Lower Clip Value", Float) = 0.0
		_OutlineStrength("Outline Strength", Float) = 0.0
		_IconStrength("Icon Strength", Float) = 0.0
		_ChargedFlash("Charged Flash", Float) = 0.0

		_PlatformElevation("Elevation", Float) = 0
		//_PlayerElevation("Player Elevation", Float) = 0
		//_MaxElevationOffset("Max Difference in Height between player and solid color", Float) = 30.0

		_Opacity("Opacity", Float) = 1.0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
		}
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert             
			#pragma fragment frag

			struct vertInput
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD;
			};

			struct vertOutput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD;
			};

			vertOutput vert(vertInput input)
			{
				vertOutput o;
				o.pos = UnityObjectToClipPos(input.pos);
				o.uv = input.uv;
				return o;
			}

			sampler2D _MainTex;
			sampler2D _GlowRampTex;
			sampler2D _GlowColorMapTex;
			float _GlowUpperClip;
			float _GlowLowerClip;
			float4 _HighColor;
			float4 _LowColor;
			float _PlatformElevation;
			float _PlayerElevation;
			float _MaxElevationOffset;
			float _Opacity;
			float _IconStrength;
			float _OutlineStrength;
			float _ChargedFlash;

			// utility inv lerp, thanks to Ronja @ https://www.ronja-tutorials.com/post/047-invlerp_remap/
			float invLerp(float from, float to, float value) {
				return (value - from) / (to - from);
			}

			float4 frag(vertOutput output) : COLOR
			{
				float4 color = tex2D(_MainTex, output.uv);
				float diff = _PlatformElevation - _PlayerElevation;
				float4 mask_color;

				// add glow stuff, using glow texture as a gradient mask
				float4 glowColor = tex2D(_GlowRampTex, output.uv);
				float pixelGlowValue = 1 - clamp(invLerp(_GlowLowerClip, _GlowUpperClip, glowColor.r), 0, 1); // oneminus cuz I did gradients in a weird but intuitive to me way
				pixelGlowValue += glowColor.g * _OutlineStrength;
				pixelGlowValue += glowColor.b * _IconStrength;
				pixelGlowValue = clamp(pixelGlowValue, 0, 1);
				color += glowColor.a * tex2D(_GlowColorMapTex, float2(pixelGlowValue, 0.5));

				color += (glowColor.a + tex2D(_GlowColorMapTex, float2(1.0, 0.5))) * _ChargedFlash; // uses mask texture's alpha to have masked area be white and unmasked be max glow color


				if (color.a != 1) return color;

				if (diff > 0.0)
				{
					mask_color = _HighColor;
				}
				else { mask_color = _LowColor; }

				diff = abs(diff);
				float ratio = diff / _MaxElevationOffset;
				if (ratio >= 1.0)
				{
					mask_color.a = _Opacity;
					return mask_color;
				}
				else
				{
					 mask_color = (1.0 - ratio) * color + ratio * mask_color;
					 mask_color.a = _Opacity;
					 return mask_color;
				}
			}
			ENDCG
		}
	}
}