Shader "Custom/SolidColorSprite"
{
    Properties
    {
        _Color ("Solid Color", Color) = (1, 1, 0, 1) // Yellow by default
        _MainTex ("Sprite Texture", 2D) = "white" {}  // The sprite texture, used only for alpha
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            sampler2D _MainTex;  // The sprite texture (for alpha)
            fixed4 _Color;       // The solid color (yellow)

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

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture to get the alpha value (preserving the shape)
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Use the solid color (yellow) and keep the alpha from the texture
                fixed4 finalColor = _Color;
                finalColor.a = texColor.a;  // Keep the sprite's alpha to preserve its shape

                return finalColor;
            }
            ENDCG
        }
    }
}
