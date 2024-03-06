// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Custom/ShinyPlatformShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_ShinyTex("ShinyTexture", 2D) = "white" {} // this texture scrolls and changes offset based on camera position
		//_HighColor("High Color", Color) = (0.0, 0.0, 0.0, 1.0)
		//_LowColor("Low Color", Color) = (1.0, 1.0, 1.0, 1.0)

		_PlatformElevation("Elevation", Float) = 0
		//_PlayerElevation("Player Elevation", Float) = 0
		//_MaxElevationOffset("Max Difference in Height between player and solid color", Float) = 30.0
		_FGColor("Foreground Color", Color) = (1,0,1,1)
		_BGColor("Background Color", Color) = (0.5,0,0,1)
		_OpaqueColor("Opaque Color", Color) = (1,1,1,1)

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
			sampler2D _ShinyTex;
			float4 _HighColor;
			float4 _LowColor;
			float _PlatformElevation;
			float _PlayerElevation;
			float _MaxElevationOffset;
			float _Opacity;
			float4 _BGColor;
			float4 _FGColor;
			float4 _OpaqueColor;

			float4 frag(vertOutput output) : COLOR
			{
				//---------------------------------------------------------------------------| scale of the texture |----|   camera scroll rate    |----|  offset by position
				//---------------------------------------------------------------------------|  high = more dense   |----|     higher = faster     |----|
				float4 color = lerp(_FGColor, _BGColor, tex2D(_ShinyTex, (output.pos * 0.0005f + _WorldSpaceCameraPos.xy * 0.012f /* + unity_ObjectToWorld._m03_m13_m23 * 0.03*/) * float2(2, 0.3)));
				color +=	   lerp(_FGColor, _BGColor, tex2D(_ShinyTex, (output.pos * 0.001f + _WorldSpaceCameraPos.xy * 0.008f /*+ unity_ObjectToWorld._m03_m13_m23 * 0.03*/) * float2(2, 0.3)));
				color +=	   lerp(_FGColor, _BGColor, tex2D(_ShinyTex, (output.pos * 0.003f + _WorldSpaceCameraPos.xy * 0.005f /*+ unity_ObjectToWorld._m03_m13_m23 * 0.03*/) * float2(2, 0.2)));
				color *= 0.333;
				color = lerp(color, _OpaqueColor, tex2D(_MainTex, output.uv));
				float src_opacity = color.a;

				//get diff for top and bottom
				float diff = _PlatformElevation - _PlayerElevation;
				float ratio = diff / _MaxElevationOffset;


				//float adjusted_uv_y = (output.uv.y - _TopSpriteRect) / (_BottomSpriteRect - _TopSpriteRect);
				//adjusted_uv_y = 0.5;


				//if (color.a != 1) return color;

				if (ratio > 0)
				{
					ratio = abs(ratio);
					color = (1 - ratio) * color + ratio * _HighColor;
				}
				else
				{
					ratio = abs(ratio);
					color = (1 - ratio) * color + ratio * _LowColor;
				}
				//color.a = _Opacity; //change this in the future so that half-transparent pixels dont get screwed up
				color.a = src_opacity * _Opacity;
				return color;
			}
			ENDCG
		}
	}
}