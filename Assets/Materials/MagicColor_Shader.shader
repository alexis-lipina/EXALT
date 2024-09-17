Shader "Unlit/MagicColor_Shader"
{
    Properties
    {
		_Color("Tint", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
		_SourceColor("Source Color", Color) = (1, 1, 1, 1)
		_Transparency("Transparency", Float) = 1.0

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
			float4 _Color;
			float4 _MagicColor;
			float4 _SourceColor;
			float _Transparency;

			float4 frag(vertOutput output) : COLOR
			{
				float4 color = tex2D(_MainTex, output.uv);
				if (abs(color.b - _SourceColor.b) < 0.2 && color.a != 0)
				{
					color = _MagicColor;
				}
				color.a = _Transparency * color.a * _Color.a;
				return color;
			}
			ENDCG
		}
	}
}
