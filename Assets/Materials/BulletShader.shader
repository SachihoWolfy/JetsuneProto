Shader "Custom/OutlinedObjectGlow_ParticleFriendly"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Float) = 0.0015
        _GlowIntensity ("Glow Intensity", Float) = 1.0
        _EnablePulse ("Enable Pulse", Float) = 1.0
    }

    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Opaque" }

        // Outline Pass (Render behind base mesh)
        Pass
        {
            Name "OUTLINE"
            Cull Front
            ZWrite On              // Write to depth for outline
            ZTest LEqual           // Outline rendered behind base mesh

            // Check if Outline color alpha is 1 for opaque rendering or transparency
            Blend SrcAlpha OneMinusSrcAlpha // Default blending (transparent by default)
            ColorMask RGB

            // Conditional blending based on Outline color alpha (to be done in C# script)

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float pulse : TEXCOORD0;
                float4 color : COLOR;
            };

            fixed _OutlineWidth;
            fixed4 _OutlineColor;
            float _GlowIntensity;
            float _EnablePulse;

            v2f vert(appdata v)
            {
                v2f o;
                float3 offset = v.normal * _OutlineWidth;
                o.pos = UnityObjectToClipPos(v.vertex + float4(offset, 0));
                o.pulse = (_EnablePulse > 0.5) ? (sin(_Time.y * 4.0 + v.vertex.y * 2.0) * 0.2 + 0.8) : 1.0;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = _OutlineColor * i.pulse * _GlowIntensity;
                col.rgb *= i.color.rgb;  // Optional tinting with particle color
                col.a = _OutlineColor.a * i.color.a; // Control outline alpha
                return col;
            }
            ENDCG
        }

        // Base Mesh Pass (Apply color tint with transparency)
        Pass
        {
            Name "BASE"
            Cull Back
            ZWrite Off              // Disable depth writing for transparency
            ZTest LEqual            // Ensure correct depth test for transparency

            // Blend based on color tint transparency
            Blend SrcAlpha OneMinusSrcAlpha // Enable alpha blending for base mesh

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Apply color tint to the base texture, including alpha transparency
                fixed4 baseCol = tex2D(_MainTex, i.uv) * _Color;
                return baseCol;
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}
