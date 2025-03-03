Shader "Gaussian Splatting/RenderPoints"
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
            #include "GaussianSplatting.hlsl"

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
                o.col.rgb = g.shs.sh0;
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
