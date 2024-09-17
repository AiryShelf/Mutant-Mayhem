Shader "Custom/FixedWorldSpaceScrollingTransparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}   // Main texture with transparency
        _ScrollSpeed ("Scroll Speed", Vector) = (0.1, 0.0, 0, 0)  // Scroll speed for the texture
        _Color ("Color", Color) = (1, 1, 1, 1)  // Color multiplier with alpha
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 200

        // Enable blending for transparency
        Blend SrcAlpha OneMinusSrcAlpha
        
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;      // The texture (with transparency)
            float4 _ScrollSpeed;     // Scrolling speed for the texture
            float4 _Color;           // Color with alpha

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Scroll UVs based on time and scrolling speed
                o.uv = v.uv + _Time.y * _ScrollSpeed.xy;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture with alpha
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Multiply by the input color (allows tinting and transparency control)
                return texColor * _Color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
