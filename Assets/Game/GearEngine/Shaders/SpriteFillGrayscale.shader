Shader "GearEngine/Sprites/SpriteFillGrayscale"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        
        _FillAmount ("Fill Amount", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment CustomSpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            float _FillAmount;

            fixed4 CustomSpriteFrag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                
                // Calculate grayscale
                fixed grayscale = dot(c.rgb, fixed3(0.299, 0.587, 0.114));
                fixed3 grayColor = fixed3(grayscale, grayscale, grayscale);

                // fill from bottom to top (UV.y goes from 0 to 1)
                float mask = step(IN.texcoord.y, _FillAmount);
                
                // Lerp between grayscale and original color based on the mask
                c.rgb = lerp(grayColor, c.rgb, mask);
                
                // Premultiply alpha (Standard Unity Sprite setup)
                c.rgb *= c.a;
                
                return c;
            }
        ENDCG
        }
    }
}
