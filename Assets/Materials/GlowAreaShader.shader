/**
    This shader is for glow overlay effects that should be applied to objects. 
*/



Shader "Unlit/GlowAreaShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_FadeGradientTex("Fade Gradient", 2D) = "white" {}
		_GlowLevel("Glow Level", Float) = 0.5
		//_GradientSize("Gradient size", Float) = 1.0
	}
		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
			}
			Pass
			{
				//ZWrite Off
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

				vertOutput vert(vertInput input)
				{
					vertOutput o;
					o.pos = UnityObjectToClipPos(input.pos);
					o.uv = input.uv;
					return o;
				}

				sampler2D _MainTex;
				sampler2D _FadeGradientTex;
				float4 _SourceColor;
				float _GlowLevel;
				float _GradientSize;

				float4 frag(vertOutput output) : COLOR
				{
					float4 color = tex2D(_MainTex, output.uv);;
					float4 outColor = tex2D(_FadeGradientTex, float2(clamp(color.b * _GlowLevel, 0, 0.99), 0.5)) * color.a;
					//float4 outColor = tex2D(_FadeGradientTex, float2(clamp(color.b, 0, 0.99), 0.5));
					//outColor.a = min(_GlowLevel - 1, outColor.a);
					outColor.a *= color.a;
					return outColor;
				}
				ENDCG
			}
		}
}
