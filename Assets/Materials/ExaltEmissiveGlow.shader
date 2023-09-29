Shader "Custom/ExaltEmissive"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
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
			//SetTexture[_MainTex] {combine texture }
				

			CGPROGRAM
			#pragma vertex vert             
			#pragma fragment frag

			struct vertInput
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD;
				fixed4 color : COLOR;
			};

			struct vertOutput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD;
				float4 color : COLOR;
			};

			vertOutput vert(vertInput input)
			{
				vertOutput o;
				o.pos = UnityObjectToClipPos(input.pos);
				o.uv = input.uv;
				o.color = input.color;
				return o;
			}

			sampler2D _MainTex;
			float _Opacity;

			float4 frag(vertOutput output) : COLOR
			{
				float4 color = tex2D(_MainTex, output.uv);
				color *= output.color * 2;
				//color *= float4(1, 1, 1, _Opacity);
				color *= _Opacity; // this way opacity over 1 lets it bloom harder
				return color;
			}
			ENDCG
		}
	}
}