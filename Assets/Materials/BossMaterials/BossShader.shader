Shader "Custom/BossShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_DepthTex("Depth Texture", 2D) = "white" {}
		_MaskTex("Crystal Mask Texture", 2D) = "white" {}
		_ShinyTex("Crystal Pattern Texture", 2D) = "white" {}
		_SparkleTex("White Sparkle Pattern", 2D) = "white" {}
		//_PlayerElevation("Player Elevation", Float) = 0
		_TopElevation("Top Elevation", Float) = 0
		_BottomElevation("Bottom Elevation", Float) = 0
		//_MaxElevationOffset("Max Difference in Height between player and solid color", Float) = 30.0
		_RippleWidth("Ripple Width", Float) = 0.2
		_RippleIntensity("Ripple Intensity", Float) = 1
		_RipplePosition("Ripple Position", Float) = 0.5

		//_HighColor("High Color", Color) = (1, 1, 1, 1)
		//_LowColor("Low Color", Color) = (0, 0, 0, 1)

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
			sampler2D _DepthTex;
			sampler2D _MaskTex;
			sampler2D _ShinyTex;
			sampler2D _SparkleTex;

			float4 _HighColor;
			float4 _LowColor;

			float _PlayerElevation;
			float _TopElevation;
			float _BottomElevation;
			float _MaxElevationOffset;

			float _RippleWidth;
			float _RippleIntensity;
			float _RipplePosition;

			float _Opacity;

			// hit flash
			float _MaskOn;
			half4 _MaskColor;

			// https://www.desmos.com/calculator/1yakozvmbd
			float DepthRipple(float depthpixel) // clamps a negative abs value graph to 0..1 range
			{
				float temptime = (_SinTime.a + 1) / 2.0f;
				//_RipplePosition = temptime;
				return clamp(_RippleIntensity - abs(2 * _RippleIntensity * (_RipplePosition - depthpixel) / _RippleWidth), 0.0f, 2.0f);
			}

			//right now just a regular gradient from top to bottom
			float4 frag(vertOutput output) : COLOR
			{
				float4 mainTexColor = tex2D(_MainTex, output.uv);
				float4 depthTexColor = tex2D(_DepthTex, output.uv);
				float depthRippleValue = DepthRipple(depthTexColor.r);
				float4 color = tex2D(_ShinyTex, (output.pos * 0.001f + _WorldSpaceCameraPos.xy * 0.012f + unity_ObjectToWorld._m03_m13_m23 * 0.03) * float2(4, 0.3)) * float4(1, 0, 1, 1);
				color += tex2D(_ShinyTex, (output.pos * 0.002f + _WorldSpaceCameraPos.xy * 0.008f + unity_ObjectToWorld._m03_m13_m23 * 0.03) * float2(4, 0.3)) * float4(1, 0, 0.5, 1);
				color += tex2D(_ShinyTex, (output.pos * 0.003f + _WorldSpaceCameraPos.xy * 0.005f + unity_ObjectToWorld._m03_m13_m23 * 0.03) * float2(4, 0.3)) * float4(1, 0, 0, 1);
				color *= 0.5;
				color *= depthRippleValue * float4(1.0f, 0.0f, 1.0f, 1.0f) + 0.5f;
				color += float4(0.2, 0.0, 0.1, 1); // baseline bg color
				//color += tex2D(_SparkleTex, (output.pos * 0.001f + _WorldSpaceCameraPos.xy * 0.02f + unity_ObjectToWorld._m03_m13_m23 * 0.03) * float2(4, 2)) * depthRippleValue * 2;
				color += max(0, depthRippleValue - 1);
				color = pow(color, depthRippleValue + 1);

				color = lerp(color, mainTexColor, tex2D(_MaskTex, output.uv));
				color.a = mainTexColor.a;
				//color = tex2D(_MainTex, output.uv);

				_MaxElevationOffset = 200.0f;
				//float4 color = tex2D(_MainTex, output.uv);
				float src_opacity = color.a;
				//get diff for top and bottom
				float top_diff = _TopElevation - _PlayerElevation;
				float bottom_diff = _BottomElevation - _PlayerElevation;

				//float adjusted_uv_y = (output.uv.y - _TopSpriteRect) / (_BottomSpriteRect - _TopSpriteRect);
				//adjusted_uv_y = 0.5;

				//float ratio = //lerp(top_diff, bottom_diff, 1 - adjusted_uv_y);
				float ratio = lerp(top_diff, bottom_diff, 1.0f - depthTexColor.r);

				//ratio = ratio / _MaxElevationOffset;
				ratio = ratio / 150.0f;

				//if (color.a != 1) return color;

				// IVE DISABLED DEPTH SHADING CUZ IT LOOKS KINDA BAD WHEN THE BOSS IS ALWAYS ABOVE YOU. IDK WHAT TO DO ABT IT TBH
				/*
				if (ratio > 0)
				{
					ratio = abs(ratio);
					color = (1 - ratio) * color + ratio * _HighColor;
				}
				else
				{
					ratio = abs(ratio);
					color = (1 - ratio) * color + ratio * _LowColor;
				}*/
				//color.a = _Opacity; //change this in the future so that half-transparent pixels dont get screwed up

				color.a = src_opacity * _Opacity;

				color = color * (1 - _MaskOn) + _MaskColor * _MaskOn;

				return color;
			}

			

			ENDCG
		}
	}
}