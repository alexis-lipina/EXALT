/// <summary>
/// This postprocess effect causes the edges of the screen to shatter like glass. It's intended for the final boss sequence where the player loses
/// max HP as they defeat fragments of the boss. 
/// </summary>
Shader "Unlit/ShatterEdges_PPS"
{
	Properties
	{
		_MainTex("Rendered Screen", 2D) = "white" {}
		_OffsetTex("Glitch Texture", 2D) = "white" {} 
		_CrackTex("Crack Texture", 2D) = "white" {} // for rendering actual texture cracks
		_ShatterMaskTex("Mask Texture", 2D) = "white" {} // for masking out dark background areas
	}

		CGINCLUDE
#include "UnityCG.cginc"

	sampler2D _MainTex;
	sampler2D _OffsetTex;
	sampler2D _CrackTex;
	sampler2D _ShatterMaskTex;

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
		float UpdateRate = 12;//per second
		float stepGlitch = fmod(floor(_Time.g * UpdateRate) / 14.9f, 1.0f);
		float GlitchOffsetScale = 0.08f;
		//scrollNoiseUV.x = fmod(abs(i.uv.x  + stepGlitch * direction * speed), 1.0f);
		half4 glitchTexOffset = tex2D(_OffsetTex, scrollNoiseUV) * GlitchOffsetScale;
		glitchTexOffset *= half4(1.0f, 0.0f, 0.0f, 0.0f); // neutralize y component, only horizontal shatter rn
		glitchTexOffset *= half4(direction, 0.0f, 0.0f, 0.0f); // flip direction

		float LerpFromMiddleTemp = 1 - step(abs(i.uv.x * 4 - 2)-1, 0);
		//i.uv += glitchTexOffset; /* * LerpFromMiddleTemp*/;

		//half4 colorMultiply = half4(1.0f, 1.0f, 1.0f, 1.0f);
		half4 colorMultiply = half4(.0f, .0f, .0f, .0f);
		float gradient = 1.0f;

		/*
		if (glitchTexOffset.r != 0 && glitchTexOffset.g != 0)
		{
			colorMultiply = half4(1.0f, 0.0f, 1.0f * (glitchTexOffset.r * 10), 0.0f);
			gradient = 1 - fmod(i.uv.y * 64, 1.0f) * 0.5f;
			//gradient += 0.3f;
		}*/

		// glow at center behind the cracks
		float normalizedCenterGlow = (-1 * abs(i.uv.x * 2 - 1) + 1) * (-1 * abs(i.uv.y * 2 - 1) + 1); // ramp up to 1 at 0.5 and down to 0 at 1 and 0
		float scaledGlow = pow(normalizedCenterGlow, 0.5) * 2.0f;
		float fluctuateTime = (-1 * abs(fmod(_Time.a, 1.0f) * 2 - 1) + 1) * ceil(-1 * abs(fmod(_Time.a * 0.5f, 2.0f) * 2 - 1) + 1);
		half4 opacityMaskPixel = fluctuateTime * half4(1.0f, 0.0f, 0.5f, 1.0f) * scaledGlow;

		//half4 finalColor = tex2D(_MainTex, i.uv - glitchTexOffset) + colorMultiply + tex2D(_OffsetTex, i.uv);
		half4 translucentColor = half4(1.0f, 0.0f, 0.5f, 1.0f);
		half4 finalColor = tex2D(_MainTex, i.uv - glitchTexOffset);
		finalColor = lerp(finalColor, translucentColor * 3.0f, tex2D(_OffsetTex, i.uv) * 0.1f);
		return (1 - tex2D(_ShatterMaskTex, i.uv)) * opacityMaskPixel + (tex2D(_ShatterMaskTex, i.uv)) * finalColor + tex2D(_CrackTex, i.uv);
		//return tex2D(_MainTex, i.uv) * half4(1, 0, 0, 0);
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
