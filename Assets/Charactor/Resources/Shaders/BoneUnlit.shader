Shader "Unlit/BoneUnlit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)    
        [Toggle(WIRE_ON)]WIRE_ON("WIRE_ON", Float) = 0

    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
        LOD 100

        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        Fog { Mode Off }
        ZTest Always

        Pass
        {
            Name "BoneUnlit"
            Tags{"LightMode" = "BoneRender"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ WIRE_ON


            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

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
            
            CBUFFER_START(UnityPerMaterial)
                    float4 _Color;
            CBUFFER_END

            v2f vert (appdata input)
            {
                v2f o;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);
                o.vertex = vertexInput.positionCS;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = _Color;
                
                #ifdef WIRE_ON
                    col.a = 1.0f;
                #endif
                
                return col;
            }
            ENDHLSL
        }
    }
}
