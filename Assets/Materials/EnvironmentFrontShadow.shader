Shader "Custom/EnvironmentFrontShadow"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_LeftBound("Left bound of shadow column", Float) = 0.5
		_RightBound("Right bound of shadow column", Float) = 0.5
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
	float _LeftBound;
	float _RightBound;

	//FRAGMENT SHADER
	float4 frag(vertOutput output) : COLOR
	{
		float4 color = tex2D(_MainTex, output.uv);
		return color * step(_LeftBound, output.uv.x) * step(output.uv.x, _RightBound);


	}
		ENDCG
	}
	}
}