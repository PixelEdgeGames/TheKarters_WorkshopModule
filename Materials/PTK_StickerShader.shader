Shader "PTK/PTK_StickerShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Angle("Angle", Range(0,360)) = 0.0
            
        _Hue("Hue", Range(0, 360)) = 0.
    } 
        
        SubShader
        {
            Offset -1, -1
            Tags {"Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 200
            CGPROGRAM

            #pragma surface surf Standard fullforwardshadows alpha:fade vertex:vert
            #pragma target 3.0

            sampler2D _MainTex;
        float4 _MainTex_ST;

            struct Input {
                float2 uv_MainTex;
                float2 MainTexRotUV;
            };

            half _Glossiness;
            float _Angle;
            half _Metallic;
            fixed4 _Color;
            float2 MainTexRotUV;

            float _Hue;

            void vert(inout appdata_full v ,out Input o) {

                UNITY_INITIALIZE_OUTPUT(Input, o);

                // Pivot
                float2 pivot = float2(0.5, 0.5);
                // Rotation Matrix
                float cosAngle = cos(-_Angle* 0.0174533);
                float sinAngle = sin(-_Angle* 0.0174533);
                float2x2 rot = float2x2(cosAngle, -sinAngle, sinAngle, cosAngle);

                // Rotation consedering pivot
                float2 fTileScale = _MainTex_ST.xy;
                float2 fTileOffset =(fTileScale - float2(1.0f, 1.0f)) * 0.5f - _MainTex_ST.zw* (fTileScale - float2(1.0f, 1.0f)); // dzieki temu przy zwiekszeniu albo zmniejszeniu skali center zostaje w centrum caly czas
                float2 uv = TRANSFORM_TEX(v.texcoord, _MainTex) - pivot- fTileOffset;
                o.MainTexRotUV = mul(rot, uv);
                o.MainTexRotUV += pivot;
            }

            inline float3 applyHue(float3 aColor, float aHue)
            {
                float angle = radians(aHue);
                float3 k = float3(0.57735, 0.57735, 0.57735);
                float cosAngle = cos(angle);
                //Rodrigues' rotation formula
                return aColor * cosAngle + cross(k, aColor) * sin(angle) + k * dot(k, aColor) * (1 - cosAngle);
            }


            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                // apply the tiling and offset (_MainTex_ST) you would normally get from using uv_MainTex
                float2 uv = IN.MainTexRotUV;// TRANSFORM_TEX(IN.MainTexRotUV, _MainTex);
                fixed4 c = tex2D(_MainTex, uv);// *_Color;

                o.Albedo = applyHue(c, _Hue);


                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Alpha = c.a;
              

            }
            ENDCG
        }
            FallBack "Standard"
}
