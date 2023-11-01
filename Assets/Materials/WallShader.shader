//This shader allows vertical objects / faces of objects to change in color as the player moves


Shader "Custom/WallShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		//_PlayerElevation("Player Elevation", Float) = 0
		_TopElevation("Top Elevation", Float) = 0
		_BottomElevation("Bottom Elevation", Float) = 0
		//_MaxElevationOffset("Max Difference in Height between player and solid color", Float) = 30.0

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

			float4 _HighColor;
			float4 _LowColor;
			
			float _PlayerElevation;
			float _TopElevation;
			float _BottomElevation;
			float _MaxElevationOffset;
			float _TopSpriteRect;
			float _BottomSpriteRect;

			float _Opacity;

			//right now just a regular gradient from top to bottom
			float4 frag(vertOutput output) : COLOR
			{

				float4 color = tex2D(_MainTex, output.uv);
				float src_opacity = color.a;
				//get diff for top and bottom
				float top_diff = _TopElevation - _PlayerElevation;
				float bottom_diff = _BottomElevation - _PlayerElevation;
				
				float adjusted_uv_y = (output.uv.y - _TopSpriteRect) / (_BottomSpriteRect - _TopSpriteRect);
				//adjusted_uv_y = 0.5;

				float ratio = lerp(top_diff, bottom_diff, 1-adjusted_uv_y);

				ratio = ratio / _MaxElevationOffset;

				//if (color.a != 1) return color;

				if (ratio > 0 )
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