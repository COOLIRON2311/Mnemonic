Shader "Gaussian Splatting/Composite"
{
    SubShader
    {
        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(uint vtxID : SV_VERTEXID)
            {
                v2f o;
                uint idx = vtxID;
                float2 quadPos = float2(idx & 1, (idx >> 1) & 1) * 2.0 - 1.0;
                o.vertex = float4(quadPos, 1, 1);
                return o;
            }

            Texture2D _GaussianRT;

            half4 frag(v2f i) : SV_Target
            {
                half4 col = _GaussianRT.Load(int3(i.vertex.xy, 0));
                col.rgb = GammaToLinearSpace(col.rgb);
                return col;
            }
            ENDCG
        }
    }
}
