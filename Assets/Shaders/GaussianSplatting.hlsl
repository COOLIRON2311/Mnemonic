#ifndef __GAUSSIAN_SPLATTING_HLSL__
#define __GAUSSIAN_SPLATTING_HLSL__

static const float SH_C0 = 0.28209479177387814;
static const float SH_C1 = 0.4886025119029199;
static const float SH_C2[] = {
    1.0925484305920792,
    - 1.0925484305920792,
    0.31539156525252005,
    - 1.0925484305920792,
    0.5462742152960396
};
static const float SH_C3[] = {
    - 0.5900435899266435,
    2.890611442640554,
    - 0.4570457994644658,
    0.3731763325901154,
    - 0.4570457994644658,
    1.445305721320277,
    - 0.5900435899266435
};

struct SHData
{
    float3 sh0;
    float3 sh1, sh2, sh3, sh4, sh5, sh6, sh7, sh8, sh9, sh10, sh11, sh12, sh13, sh14, sh15;
};

struct Gaussian
{
    float3 pos;
    float3 norm;
    SHData shs;
    float opacity;
    float3 scale;
    float4 rot;
};

struct GSViewData
{
    float4 pos;
    float4 conicRadius;
    float4 color;
};

// Convert the input spherical harmonics coefficients of each Gaussian to a simple RGB color.
// https://github.com/graphdeco-inria/diff-gaussian-rasterization/blob/main/cuda_rasterizer/forward.cu#L20
float3 GS_computeColorFromSH(SHData shs, half3 dir, int deg)
{
    dir = -dir;

    float3 result = SH_C0 * shs.sh0;

    if (deg > 0)
    {
        float x = dir.x;
        float y = dir.y;
        float z = dir.z;
        result += SH_C1 * (-y * shs.sh1 + z * shs.sh2 - x * shs.sh3);

        if (deg > 1)
        {
            float xx = x * x, yy = y * y, zz = z * z;
            float xy = x * y, yz = y * z, xz = x * z;
            result += SH_C2[0] * xy * shs.sh4 +
            SH_C2[1] * yz * shs.sh5 +
            SH_C2[2] * (2.0 * zz - xx - yy) * shs.sh6 +
            SH_C2[3] * xz * shs.sh7 +
            SH_C2[4] * (xx - yy) * shs.sh8;

            if (deg > 2)
            {
                result += SH_C3[0] * y * (3.0 * xx - yy) * shs.sh9 +
                SH_C3[1] * xy * z * shs.sh10 +
                SH_C3[2] * y * (4.0 * zz - xx - yy) * shs.sh11 +
                SH_C3[3] * z * (2.0 * zz - 3.0 * xx - 3.0 * yy) * shs.sh12 +
                SH_C3[4] * x * (4.0 * zz - xx - yy) * shs.sh13 +
                SH_C3[5] * z * (xx - yy) * shs.sh14 +
                SH_C3[6] * x * (xx - 3.0 * yy) * shs.sh15;
            }
        }
    }
    result += 0.5;

    return max(result, 0.0);
}

// 2D covariance matrix computation
// https://github.com/graphdeco-inria/diff-gaussian-rasterization/blob/main/cuda_rasterizer/forward.cu#L74
float3 GS_computeCov2D(float3 pos, float2 focal, float2 tanFov, float3 cov3D0, float3 cov3D1, float4x4 viewMatrix)
{
    float3 t = mul(viewMatrix, float4(pos, 1)).xyz;

    const float limX = 1.3 * tanFov.x;
    const float limY = 1.3 * tanFov.y;
    t.x = clamp(t.x / t.z, -limX, limX) * t.z;
    t.y = clamp(t.y / t.z, -limY, limY) * t.z;

    float3x3 J = float3x3(
        focal.x / t.z, 0.0, - (focal.x * t.x) / (t.z * t.z),
        0.0, focal.y / t.z, - (focal.y * t.y) / (t.z * t.z),
        0, 0, 0
    );

    float3x3 W = float3x3(
        viewMatrix._m00, viewMatrix._m01, viewMatrix._m02,
        viewMatrix._m10, viewMatrix._m11, viewMatrix._m12,
        viewMatrix._m20, viewMatrix._m21, viewMatrix._m22
    );

    float3x3 T = mul(J, W);

    float3x3 Vrk = float3x3(
        cov3D0.x, cov3D0.y, cov3D0.z,
        cov3D0.y, cov3D1.x, cov3D1.y,
        cov3D0.z, cov3D1.y, cov3D1.z
    );

    float3x3 cov = mul(T, mul(Vrk, transpose(T)));

    // Apply low-pass filter: every Gaussian should be at least
	// one pixel wide/high. Discard 3rd row and column.
    cov._m00 += 0.3;
    cov._m11 += 0.3;
    return float3(cov._m00, cov._m01, cov._m11);
}

// Convert scale and rotation properties of each Gaussian to a 3D covariance matrix in world space
// https://github.com/graphdeco-inria/diff-gaussian-rasterization/blob/main/cuda_rasterizer/forward.cu#L118
void GS_computeCov3D(float3x3 M, out float3 cov3D0, out float3 cov3D1)
{
    // Compute 3D world covariance matrix Sigma
    float3x3 Sigma = mul(M, transpose(M));

    // Covariance is symmetric, only store upper right
    cov3D0 = float3(Sigma._m00, Sigma._m01, Sigma._m02);
    cov3D1 = float3(Sigma._m11, Sigma._m12, Sigma._m22);
}

float3x3 computeModelMatrix(float3 scale, float mod, float4 rot)
{
    // Create scaling matrix
    float3x3 S = float3x3(
        mod * scale.x, 0, 0,
        0, mod * scale.y, 0,
        0, 0, mod * scale.z
    );

    float r = rot.w;
    float x = rot.x;
    float y = rot.y;
    float z = rot.z;

    // Compute rotation matrix from quaternion
    float3x3 R = float3x3(
        1.0 - 2.0 * (y * y + z * z), 2.0 * (x * y - r * z), 2.0 * (x * z + r * y),
        2.0 * (x * y + r * z), 1.0 - 2.0 * (x * x + z * z), 2.0 * (y * z - r * x),
        2.0 * (x * z - r * y), 2.0 * (y * z + r * x), 1.0 - 2.0 * (x * x + y * y)
    );

    float3x3 M = mul(R, S);
    return M;
}

// https://github.com/graphdeco-inria/diff-gaussian-rasterization/blob/main/cuda_rasterizer/forward.cu#L350
float2 computeScreenSpaceDelta(float2 positionXY, float2 centerXY, float4 projectionParams)
{
    float2 d = positionXY - centerXY;
    d.y *= projectionParams.x;
    return d;
}

// https://github.com/graphdeco-inria/diff-gaussian-rasterization/blob/main/cuda_rasterizer/forward.cu#L352
float computePowerFromConic(float3 conic, float2 d)
{
    return -0.5 * (conic.x * d.x * d.x + conic.z * d.y * d.y) + conic.y * d.x * d.y;
}

#endif // __GAUSSIAN_SPLATTING_HLSL__
