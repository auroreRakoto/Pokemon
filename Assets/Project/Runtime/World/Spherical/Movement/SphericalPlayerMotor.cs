using UnityEngine;

namespace Pokemon.Project.World.Spherical.Movement
{
    /// <summary>Small deterministic movement integrator. Velocity is expressed in world metres/second.</summary>
    public sealed class SphericalPlayerMotor
    {
        public Vector3 Velocity { get; private set; }
        public bool IsGrounded { get; private set; }

        public void SetGrounded(Vector3 groundNormal)
        {
            IsGrounded = true;
            float intoGround = Vector3.Dot(Velocity, groundNormal);
            if (intoGround < 0f) Velocity -= groundNormal * intoGround;
        }

        public void SetAirborne() => IsGrounded = false;

        public void Accelerate(Vector3 desiredTangentVelocity, Vector3 radialUp, float acceleration, float deltaTime)
        {
            Vector3 currentTangent = Vector3.ProjectOnPlane(Velocity, radialUp);
            Vector3 radialVelocity = Velocity - currentTangent;
            Vector3 next = Vector3.MoveTowards(currentTangent, desiredTangentVelocity, Mathf.Max(0f, acceleration) * deltaTime);
            Velocity = next + radialVelocity;
        }

        public void AddJumpImpulse(Vector3 groundNormal, float impulse)
        {
            if (!IsGrounded) return;
            Velocity += groundNormal.normalized * Mathf.Max(0f, impulse);
            IsGrounded = false;
        }

        public void IntegrateGravity(Vector3 planetCentre, Vector3 position, float gravity, float deltaTime)
        {
            if (IsGrounded) return;
            Vector3 inward = (planetCentre - position).normalized;
            Velocity += inward * Mathf.Max(0f, gravity) * deltaTime;
        }

        public void IntegratePosition(ref Vector3 position, float deltaTime) => position += Velocity * deltaTime;

        public void RemoveVelocityIntoSurface(Vector3 outwardNormal)
        {
            float intoSurface = Vector3.Dot(Velocity, outwardNormal);
            if (intoSurface < 0f) Velocity -= outwardNormal * intoSurface;
        }

        public void SetVelocity(Vector3 velocity) => Velocity = velocity;
    }
}
