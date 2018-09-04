// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Custom/PlatformShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_HighColor("High Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_LowColor("Low Color", Color) = (1.0, 1.0, 1.0, 1.0)

		_PlatformElevation("Elevation", Float) = 0
		_PlayerElevation("Player Elevation", Float) = 0
		_MaxElevationOffset("Max Difference in Height between player and solid color", Float) = 30.0
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
			float _PlatformElevation;
			float _PlayerElevation;
			float _MaxElevationOffset;

			float4 frag(vertOutput output) : COLOR
			{
				float4 color = tex2D(_MainTex, output.uv);
				float diff = _PlatformElevation - _PlayerElevation;
				float4 mask_color;

				if (color.a != 1) return color;

				if (diff > 0.0)
				{
					mask_color = _LowColor;
				}
				else { mask_color = _HighColor; }

				diff = abs(diff);
				float ratio = diff / _MaxElevationOffset;
				if (ratio >= 1.0) return mask_color;
				else
				{
					return (1.0 - ratio) * color + ratio * mask_color;
				}

				
			}
			ENDCG
		}
	}
}