Shader "Custom/Simple1"
{
    Properties
    {
        _Color ("Albedo Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGBA)", 2D) = "white" {}
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _EmissionTex ("Emission Map", 2D) = "black" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
        ZWrite Off // Don't write to depth buffer for proper transparency
        Cull Off // Render both sides (optional)

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing // Enable GPU instancing
            
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // GPU Instancing
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _EmissionTex;
            float4 _Color;
            float4 _EmissionColor;
            float _Cutoff;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 albedo = tex2D(_MainTex, i.uv) * _Color;
                fixed4 emission = tex2D(_EmissionTex, i.uv) * _EmissionColor;

                if (albedo.a < _Cutoff) discard; // Alpha cutoff

                return fixed4(albedo.rgb + emission.rgb, albedo.a);
            }
            ENDCG
        }
    }

    FallBack "Unlit/Transparent"
}
