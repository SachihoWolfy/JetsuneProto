Shader "Custom/Bulleterasu V1"
{
    Properties
    {
        _MainTex ("Grayscale Texture", 2D) = "white" {}
        _WhiteColor ("Color for White", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        [Enum(Cutout, 0, Additive, 1)] _Mode ("Rendering Mode", Float) = 0
        [Enum(Cutout, 5, Additive, 1)] _BlendSrc ("Source Blend", Float) = 5 // Default: SrcAlpha
        [Enum(Cutout, 10, Additive, 1)] _BlendDst ("Destination Blend", Float) = 10 // Default: OneMinusSrcAlpha
        [Enum(Cutout, 1, Additive, 0)] _ZWriteSrc ("Depth Buffer", float) = 1
    }

    SubShader
    {
        Tags { 
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "DisableBatching" = "True"
        }

        Pass
        {
            Name "MainPass"
            Blend [_BlendSrc] [_BlendDst]
            ZWrite [_ZWriteSrc]
            Cull Off

            // Blending and Z settings controlled in shader
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_particles
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _WhiteColor;
            float _Cutoff;
            float _Mode;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, i.uv);
                float grayscale = texCol.r;
                float alpha = texCol.a * i.color.a;
                float3 color = lerp(i.color.rgb, _WhiteColor.rgb, grayscale);

                // Cutout
                if (_Mode < 0.5)
                {
                    clip(alpha - _Cutoff);
                    return fixed4(color, 1.0); // opaque
                }
                else // Additive
                {
                    float alpha = texCol.a * i.color.a;
                    float3 outColor = color * alpha;
                    clip(alpha - 0.0001);
                    return fixed4(outColor, alpha);
                }
            }
            ENDCG
        }
    }

    Fallback "Legacy Shaders/VertexLit"
}
