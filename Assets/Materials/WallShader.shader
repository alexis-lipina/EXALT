// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


//This shader allows vertical objects / faces of objects to change in color as the player moves


Shader "Custom/WallShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_PlayerElevation("Player Elevation", Float) = 0
		_TopElevation("Top Elevation", Float) = 0
		_BottomElevation("Bottom Elevation", Float) = 0
		_MaxElevationOffset("Max Difference in Height between player and solid color", Float) = 30.0

		_HighColor("High Color", Color) = (1, 1, 1, 1)
		_LowColor("Low Color", Color) = (0, 0, 0, 1)
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

			//right now just a regular gradient from top to bottom
			float4 frag(vertOutput output) : COLOR
			{

				float4 color = tex2D(_MainTex, output.uv);
				
				//get diff for top and bottom
				float top_diff = _TopElevation - _PlayerElevation;
				float bottom_diff = _BottomElevation - _PlayerElevation;
				
				float ratio = lerp(top_diff, bottom_diff, 1-output.uv.y);

				ratio = ratio / _MaxElevationOffset;

				if (color.a != 1) return color;

				if (ratio > 0 )
				{
					color = (1 - ratio) * color + ratio * _HighColor;
				}
				else
				{
					ratio = abs(ratio);
					color = (1 - ratio) * color + ratio * _LowColor;
				}
				
				return color;
			}

			ENDCG
		}
	}
}