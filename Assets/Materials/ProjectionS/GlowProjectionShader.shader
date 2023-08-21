Shader "Unlit/GlowProjectionShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_CullRect("Culling Rectangle (draw inside)", Color) = (0.0, 0.0, 0.0, 0.0)
		_Opacity("Opacity", Float) = 1
		_ColorOverride("Color Override (white for none)", Color) = (1.0, 1.0, 1.0, 1.0)
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
			}
			Pass
			{
				Blend SrcAlpha One

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
				float _Opacity;
				float4 _ColorOverride;

				//FRAGMENT SHADER
				float4 frag(vertOutput output) : COLOR
				{
					float4 color = tex2D(_MainTex, output.uv);
					color.a = color.a * _Opacity;
					color = color * _ColorOverride;
					float4 rect = _CullRect;
					return color * step(rect.r, output.uv.x) * step(output.uv.x, rect.b) * step(rect.g, output.uv.y) * step(output.uv.y, rect.a);


				}
				ENDCG
			}
		}
}