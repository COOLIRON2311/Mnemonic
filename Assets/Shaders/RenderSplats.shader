Shader "Gaussian Splatting/RenderSplats"
{
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            ZWrite Off
            Blend OneMinusDstAlpha One
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma require compute

            #include "GaussianSplatting.hlsl"

            StructuredBuffer<uint> _OrderBuffer;

            struct v2f
            {
                half4 col : COLOR0;
                float2 centerScreenPos : TEXCOORD3;
                float3 conic : TEXCOORD4;
                float4 vertex : SV_POSITION;
            };

            StructuredBuffer<GSViewData> _GSViewBuffer;

            v2f vert(uint vtxID : SV_VERTEXID, uint instID : SV_INSTANCEID)
            {
                v2f o;
                instID = _OrderBuffer[instID];
                GSViewData view = _GSViewBuffer[instID];
                o.col = view.color;
                o.conic = view.conicRadius.xyz;

                float4 centerClipPos = view.pos;
                o.centerScreenPos = (
                    centerClipPos.xy / centerClipPos.w *
                    float2(0.5, 0.5 * _ProjectionParams.x) + 0.5
                ) * _ScreenParams.xy;

                uint idx = vtxID;
                // https://x.com/SebAaltonen/status/1315985267258519553
                float2 quadPos = float2(idx & 1, (idx >> 1) & 1) * 2.0 - 1.0;

                float radius = view.conicRadius.w;
                float2 deltaScreenPos = quadPos * radius * 2 / _ScreenParams.xy;
                o.vertex = centerClipPos;
                o.vertex.xy += deltaScreenPos * centerClipPos.w;

                if (centerClipPos.w <= 0) // behind camera
                    o.vertex = 0;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 d = computeScreenSpaceDelta(i.vertex.xy, i.centerScreenPos, _ProjectionParams);
                float power = computePowerFromConic(i.conic, d);
                i.col.a *= saturate(exp(power));
                if (i.col.a < 1.0 / 255.0)
                    discard;

                return half4(i.col.rgb * i.col.a, i.col.a);
            }
            ENDCG
        }
    }
}
