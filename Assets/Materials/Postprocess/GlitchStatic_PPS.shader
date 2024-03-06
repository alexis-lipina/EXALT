/// <summary>
/// This postprocess shader creates an offset-glitch effect with horizontal slices. It can slowly ramp up over time
/// </summary>
Shader "Unlit/GlitchStatic_PPS"
{
	Properties
	{
		_MainTex("Rendered Screen", 2D) = "white" {}
		_GlitchOffsetTex("Glitch Offset Texture", 2D) = "white" {} 
	}

		CGINCLUDE
#include "UnityCG.cginc"

	sampler2D _MainTex;
	sampler2D _GlitchOffsetTex;
	float _GlitchOffsetStrength = 1.0f; // lower values make the UV-offset weaker, if you want a more subtle offset effect
	float _GlitchOffsetMask = 1.0f; // values in the texture that are above this are ignored. used if you want less rectangle glitches 
	float _GlitchTime = 0.0f;

	struct VertexData 
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct Interpolators 
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	Interpolators VertexProgram(VertexData v) 
	{
		Interpolators i;
		i.pos = UnityObjectToClipPos(v.vertex);
		i.uv = v.uv;
		return i;
	}

	half4 FragmentProgram(Interpolators i) : SV_Target
	{
		//  sample color, fill horizontal line
		float2 scrollNoiseUV = i.uv;
		float direction = sign(i.uv.x - 0.5f);
		float speed = 10;
		float UpdateRate = 20;//per second
		//float stepGlitch = fmod(floor(_Time.g * UpdateRate) / 14.9f, 1.0f);
		float stepGlitch = fmod(floor(_GlitchTime * UpdateRate) / 14.9f, 1.0f);
		float GlitchOffsetScale = _GlitchOffsetStrength;
		scrollNoiseUV.x = fmod(abs(i.uv.x  + stepGlitch * speed), 1.0f);
		//scrollNoiseUV.y = fmod(abs(i.uv.x  + stepGlitch * speed), 1.0f);
		half4 glitchTexOffset = (tex2D(_GlitchOffsetTex, scrollNoiseUV) - 0.5f)* GlitchOffsetScale;
		glitchTexOffset *= half4(1.0f, 0.5f, 0.0f, 0.0f); // neutralize y component, only horizontal shatter rn
		//glitchTexOffset += 0.5f; // neutralize y component, only horizontal shatter rn

		// mask out values above the mask value
		glitchTexOffset *= step(glitchTexOffset.r, _GlitchOffsetMask);


		half4 finalColor = tex2D(_MainTex, i.uv - glitchTexOffset);
		return finalColor;
	}
	ENDCG

	SubShader
	{
		Cull Off
		ZTest Always
		ZWrite Off
		
		Pass 
		{
			CGPROGRAM
			#pragma vertex VertexProgram
			#pragma fragment FragmentProgram
			ENDCG
		}
	}
}
