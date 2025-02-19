// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "invisible sprite"
{
	Properties
	{
	[HideInInspector] _Color ("Tint", Color) = (0,0,0,0)
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		ColorMask 0
		Cull Off
		Lighting Off
		ZWrite Off
		AlphaToMask On

		Pass
		{
		CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag
            #pragma target 2.0
            #include "UnitySprites.cginc"
		ENDCG
		}
	}
}
