//This shader allows vertical objects / faces of objects to change in color as the player moves


Shader "Custom/WallShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {} // this texture (the standard material texture) is superimposed on the other, scrolling texture. 
		_ShinyTex("ShinyTexture", 2D) = "white" {} // this texture scrolls and changes offset based on camera position
		_TopElevation("Top Elevation", Float) = 0
		_BottomElevation("Bottom Elevation", Float) = 0
		_Opacity("Opacity", Float) = 1.0
		_OcclusionOpacity("Opacity", Float) = 1.0
		_FGColor("Foreground Color", Color) = (1,0,1,1)
		_BGColor("Background Color", Color) = (0.5,0,0,1)
		_OpaqueColor("Opaque Color", Color) = (1,1,1,1)

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
			float4 _BGColor;
			float4 _FGColor;
			float4 _OpaqueColor;

			float _PlayerElevation;
			float _TopElevation;
			float _BottomElevation;
			float _MaxElevationOffset;
			float _TopSpriteRect;
			float _BottomSpriteRect;

			float _Opacity;
			float _OcclusionOpacity;

			//right now just a regular gradient from top to bottom
			float4 frag(vertOutput output) : COLOR
			{
				//---------------------------------------------------------------------------| scale of the texture |----|   camera scroll rate    |----|  offset by position
				//---------------------------------------------------------------------------|  high = more dense   |----|     higher = faster     |----|
				float4 color =	lerp(_FGColor, _BGColor, tex2D(_ShinyTex, (output.pos * 0.0005f + _WorldSpaceCameraPos.xy * 0.012f/* + unity_ObjectToWorld._m03_m13_m23 * 0.03*/) * float2(2, 0.3)));
				color +=		lerp(_FGColor, _BGColor, tex2D(_ShinyTex, (output.pos * 0.001f + _WorldSpaceCameraPos.xy * 0.008f/* + unity_ObjectToWorld._m03_m13_m23 * 0.03*/) * float2(2, 0.3)));
				color +=		lerp(_FGColor, _BGColor, tex2D(_ShinyTex, (output.pos * 0.003f + _WorldSpaceCameraPos.xy * 0.005f/* + unity_ObjectToWorld._m03_m13_m23 * 0.03*/) * float2(2, 0.2)));
				color *= 0.333;
				color = lerp(color, tex2D(_MainTex, output.uv), tex2D(_MainTex, output.uv).a);
				color.a = 1.0f;
				float src_opacity = color.a;

				//get diff for top and bottom
				float top_diff = _TopElevation - _PlayerElevation;
				float bottom_diff = _BottomElevation - _PlayerElevation;

				float adjusted_uv_y = (output.uv.y - _TopSpriteRect) / (_BottomSpriteRect - _TopSpriteRect);
				//adjusted_uv_y = 0.5;

				float ratio = lerp(top_diff, bottom_diff, 1 - adjusted_uv_y);

				ratio = ratio / _MaxElevationOffset;

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
				color.a = src_opacity * _Opacity * _OcclusionOpacity;
				return color;
			}

			ENDCG
		}
	}
}