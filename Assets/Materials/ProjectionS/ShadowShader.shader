// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Custom/ShadowShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_CullRect("Culling Rectangle (draw inside)", Color) = (0.0, 0.0, 0.0, 0.0)
		_Elevation("Elevation of Shadow", Float) = 0.0
		_MaxElevationOffset("Max Difference in Height between player and solid color", Float) = 30.0


		_HighColor("High Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_LowColor("Low Color", Color) = (1.0, 1.0, 1.0, 1.0)

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

			//VERTEX SHADER
			vertOutput vert(vertInput input)
			{
				vertOutput o;
				o.pos = UnityObjectToClipPos(input.pos);
				o.uv = input.uv;
				return o;
			}

			sampler2D _MainTex;
			float4 _CullRect;

			//apply depth color effect
			float _PlayerElevation; 
			float _Elevation;
			float4 _HighColor;
			float4 _LowColor;
			float _MaxElevationOffset;
			float _Opacity;

			//FRAGMENT SHADER
			float4 frag(vertOutput output) : COLOR
			{
				float4 color = tex2D(_MainTex, output.uv);
				float4 rect = _CullRect;
				float original_opacity = color.a;
				float imposed_opacity = _Opacity;

				float diff = _Elevation - _PlayerElevation;
				float4 mask_color;

				//coloring
				if (diff > 0.0)
				{
					mask_color = _HighColor;
				}
				else { mask_color = _LowColor; }

				diff = abs(diff);
				float ratio = diff / _MaxElevationOffset;
				if (ratio >= 1.0)
				{
					mask_color.a = original_opacity * imposed_opacity;
					//return mask_color;
				}
				else
				{
					mask_color = (1.0 - ratio) * color + ratio * mask_color;
					mask_color.a = original_opacity * imposed_opacity;
					//return mask_color;
				}

				//culling
				
				return mask_color * step(rect.r, output.uv.x) * step(output.uv.x, rect.b) * step(rect.g, output.uv.y) * step(output.uv.y, rect.a); //cull



			}
			ENDCG
		}
	}
}