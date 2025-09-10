Shader "Hidden/VaporwaveEdge"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EdgeThreshold ("Edge Threshold", Range(0, 1)) = 0.2

        _Color1 ("Vapor Color 1", Color) = (1.0, 0.4, 0.7, 1.0)
        _Color2 ("Vapor Color 2", Color) = (0.4, 1.0, 1.0, 1.0)
        _Color3 ("Vapor Color 3", Color) = (0.6, 0.3, 1.0, 1.0)

        _EdgeColor ("Edge Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _EdgeStrength ("Edge Strength", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _EdgeThreshold;
            float _EdgeStrength;

            fixed4 _Color1;
            fixed4 _Color2;
            fixed4 _Color3;
            fixed4 _EdgeColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float luminance(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            float3 ApplyPalette(float luma)
            {
                // Use luminance to determine color bands
                float x = sin(luma * 20);
                return lerp(_Color1.rgb, _Color2.rgb, x);
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Get base scene brightness
                float3 baseColor = tex2D(_MainTex, uv).rgb;
                float luma = luminance(baseColor);

                // Sobel edge detection based on luminance
                float gx = 0.0;
                float gy = 0.0;

                gx += luminance(tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2(-1, -1)).rgb) * -1.0;
                gx += luminance(tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2( 1, -1)).rgb) *  1.0;
                gx += luminance(tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2(-1,  0)).rgb) * -2.0;
                gx += luminance(tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2( 1,  0)).rgb) *  2.0;
                gx += luminance(tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2(-1,  1)).rgb) * -1.0;
                gx += luminance(tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2( 1,  1)).rgb) *  1.0;

                gy += luminance(tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2(-1, -1)).rgb) * -1.0;
                gy += luminance(tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2( 0, -1)).rgb) * -2.0;
                gy += luminance(tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2( 1, -1)).rgb) * -1.0;
                gy += luminance(tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2(-1,  1)).rgb) *  1.0;
                gy += luminance(tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2( 0,  1)).rgb) *  2.0;
                gy += luminance(tex2D(_MainTex, uv + _MainTex_TexelSize.xy * float2( 1,  1)).rgb) *  1.0;

                float edge = sqrt(gx * gx + gy * gy);
                float edgeMask = edge > _EdgeThreshold ? 1.0 : 0.0;

                float3 fillColor = ApplyPalette(luma);
                float3 finalColor = lerp(fillColor, _EdgeColor.rgb, edgeMask * _EdgeStrength);

                return float4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}
