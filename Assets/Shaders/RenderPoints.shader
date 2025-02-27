Shader "Unlit/RenderPoints"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma require compute

            #include "UnityCG.cginc"

            struct Gaussian
            {
                float3 pos;
                float3 norm;
                float3 sh0;
                float3 sh1, sh2, sh3, sh4, sh5, sh6, sh7, sh8, sh9, sh10, sh11, sh12, sh13, sh14, sh15;
                float opacity;
                float3 scale;
                float4 rot;
            };

            StructuredBuffer<Gaussian> _DataBuffer;

            struct v2f
            {
                half4 col : COLOR0;
                float4 vertex : SV_POSITION;
                float4 psize : PSIZE;
            };

            v2f vert(uint vtxID : SV_VERTEXID, uint instID : SV_INSTANCEID)
            {
                v2f o;
                Gaussian g = _DataBuffer[instID];
                o.vertex = UnityObjectToClipPos(g.pos);
                o.col.rgb = g.sh0;
                o.col.a = g.opacity / 255;
                o.psize = 10;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.col;
            }
            ENDCG
        }
    }
}
