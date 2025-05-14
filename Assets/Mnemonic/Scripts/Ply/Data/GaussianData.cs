using UnityEngine;

namespace Ply
{
    public struct GaussianData
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Dc0;
        public Vector3 Sh0, Sh1, Sh2, Sh3, Sh4, Sh5, Sh6, Sh7, Sh8, Sh9, Sh10, Sh11, Sh12, Sh13, Sh14;
        public float Opacity;
        public Vector3 Scale;
        public Quaternion Rotation;
    }
}
