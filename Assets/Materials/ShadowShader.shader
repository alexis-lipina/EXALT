// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Custom/ShadowShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_CullRect("Culling Rectangle (draw inside)", Color) = (0.0, 0.0, 0.0, 0.0)
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
			
			//FRAGMENT SHADER
			float4 frag(vertOutput output) : COLOR
			{
				float4 color = tex2D(_MainTex, output.uv);
				float4 rect = _CullRect;
				return color * step(rect.r, output.uv.x) * step(output.uv.x, rect.b) * step(rect.g, output.uv.y) * step(output.uv.y, rect.a);


			}
			ENDCG
		}
	}
}