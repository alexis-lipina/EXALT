




Shader "Custom/GlowShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		/*
	    _HighColor("High Color", Color) = (0.0, 0.0, 0.0, 1.0)
		_LowColor("Low Color", Color) = (1.0, 1.0, 1.0, 1.0)

		_PlatformElevation("Elevation", Float) = 0
		_PlayerElevation("Player Elevation", Float) = 0
		_MaxElevationOffset("Max Difference in Height between player and solid color", Float) = 30.0
		*/
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
			SetTexture[_MainTex] {combine texture }
				
		}
	}
}