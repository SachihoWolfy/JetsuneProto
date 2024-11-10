Shader "Custom/TouhouBulletLocalGradientDebug"
{
    Properties
    {
        _MainColor("Outer Color", Color) = (1, 0, 0, 1) // Outer color (red)
        _CenterColor("Center Color", Color) = (1, 1, 1, 1) // Inner color (white)
        _Radius("Radius", Range(0.0, 1.0)) = 0.2 // Radius of the white center
    }
        SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 localPos : TEXCOORD1; // Store local position in object space
            };

            fixed4 _MainColor;
            fixed4 _CenterColor;
            float _Radius;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // Object to clip space
                o.localPos = v.vertex.xyz; // Store vertex position in object space
                o.uv = v.uv; // Pass UV coordinates (if needed)
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Debug: visualize the distance for troubleshooting
                float dist = length(i.localPos); // Calculate distance from center of bullet in local space

                // DEBUG: Color based on distance for visualization
                fixed4 debugColor = fixed4(dist, 0.0, 0.0, 1.0); // Red color based on distance

                // Apply the gradient: smooth transition from white to outer color
                fixed4 color = lerp(_CenterColor, _MainColor, smoothstep(_Radius, 1.0, dist));

                // Apply alpha fade for a glowing effect at the edges
                color.a *= smoothstep(0.4, 1.0, dist);

                // Return the final color (for debugging, return the debugColor to check distance visualization)
                return debugColor; // Change to `color` for the final result once we are confident
            }
            ENDCG
        }
    }
        FallBack "Diffuse"
}
