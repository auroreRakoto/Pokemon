using NUnit.Framework;
using Pokemon.Project.World.Spherical.Movement;
using UnityEngine;

namespace Pokemon.Project.Tests.World.Spherical.Movement
{
    public sealed class SphericalPlayerMotorTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void GravityAlwaysAcceleratesTowardPlanetCentre(int directionIndex)
        {
            Vector3 radialDirection = directionIndex switch
            {
                0 => Vector3.right,
                1 => Vector3.left,
                2 => Vector3.up,
                3 => Vector3.down,
                4 => Vector3.forward,
                _ => Vector3.back
            };
            var motor = new SphericalPlayerMotor();
            Vector3 position = radialDirection * 100f;
            motor.SetAirborne();
            motor.IntegrateGravity(Vector3.zero, position, 10f, 0.1f);
            Assert.That(Vector3.Dot(motor.Velocity, -radialDirection), Is.EqualTo(1f).Within(1e-5f));
        }

        [Test]
        public void JumpImpulseIsIgnoredWhenAirborne()
        {
            var motor = new SphericalPlayerMotor();
            motor.SetAirborne();
            motor.AddJumpImpulse(Vector3.up, 5f);
            Assert.That(motor.Velocity, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void JumpImpulseUsesGroundNormalWhenGrounded()
        {
            var motor = new SphericalPlayerMotor();
            Vector3 normal = new Vector3(0.2f, 0.97f, 0.1f).normalized;
            motor.SetGrounded(normal);
            motor.AddJumpImpulse(normal, 5f);
            Assert.That(Vector3.Dot(motor.Velocity, normal), Is.EqualTo(5f).Within(1e-4f));
            Assert.That(motor.IsGrounded, Is.False);
        }

        [Test]
        public void GroundAccelerationPreservesRadialVelocityComponent()
        {
            var motor = new SphericalPlayerMotor();
            motor.SetVelocity(Vector3.up * 3f);
            motor.Accelerate(Vector3.forward * 2f, Vector3.up, 10f, 0.1f);
            Assert.That(Vector3.Dot(motor.Velocity, Vector3.up), Is.EqualTo(3f).Within(1e-5f));
            Assert.That(Vector3.Dot(motor.Velocity, Vector3.forward), Is.EqualTo(1f).Within(1e-5f));
        }

        [Test]
        public void GroundContactRemovesOnlyVelocityIntoSurface()
        {
            var motor = new SphericalPlayerMotor();
            motor.SetVelocity(new Vector3(2f, -3f, 1f));
            motor.SetGrounded(Vector3.up);
            Assert.That(motor.Velocity, Is.EqualTo(new Vector3(2f, 0f, 1f)));
        }
    }
}
