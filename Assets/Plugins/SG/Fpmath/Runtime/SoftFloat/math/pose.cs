using System;

namespace ME.ECS.Mathematics
{
    [Serializable]
    public struct pose : IEquatable<pose>
    {
        public float3 position;
        public quaternion rotation;
        private static readonly pose k_Identity = new pose(float3.zero, quaternion.identity);

        public pose(float3 position, quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public override string ToString()
        {
            return $"({(object)position.ToString()}, {(object)rotation.ToString()})";
        }

        public pose GetTransformedBy(pose lhs)
        {
            return new pose()
            {
                position = lhs.position + lhs.rotation * position,
                rotation = lhs.rotation * rotation
            };
        }

        public float3 forward => rotation * float3.forward;
        public float3 right => rotation * float3.right;
        public float3 up => rotation * float3.up;
        public static pose identity => k_Identity;

        public override bool Equals(object obj) => obj is pose other && Equals(other);

        public bool Equals(pose other)
        {
            return position.Equals(other.position) && rotation.Equals(other.rotation);
        }

        public override int GetHashCode()
        {
            return position.GetHashCode() ^ rotation.GetHashCode() << 1;
        }

        public static bool operator ==(pose a, pose b) => a.Equals(b);

        public static bool operator !=(pose a, pose b) => !(a == b);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static implicit operator pose(UnityEngine.Pose v)
        {
            return new pose((float3)v.position, (quaternion)v.rotation);
        }
    }
}