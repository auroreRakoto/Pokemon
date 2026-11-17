using NUnit.Framework;
using UnityEngine;
using Pokemon.Project.World.Spherical;

namespace Pokemon.Project.Tests.World.Spherical
{
    public sealed class SphericalHeightFieldTests
    {
        [Test]
        public void SameSeedAndSettingsProduceSameHeight()
        {
            SphericalWorldSettings settings = new SphericalWorldSettings { seed = 12345 };
            Vector3 direction = new Vector3(0.3f, 0.7f, -0.2f).normalized;

            float first = SphericalHeightField.SampleHeight(direction, settings);
            float second = SphericalHeightField.SampleHeight(direction, settings);

            Assert.AreEqual(first, second);
        }

        [Test]
        public void DifferentSeedsProduceDifferentHeightField()
        {
            SphericalWorldSettings firstSettings = new SphericalWorldSettings { seed = 12345 };
            SphericalWorldSettings secondSettings = new SphericalWorldSettings { seed = 54321 };
            Vector3[] directions =
            {
                Vector3.right,
                Vector3.up,
                Vector3.forward,
                new Vector3(0.2f, 0.7f, 0.4f).normalized,
                new Vector3(-0.6f, 0.1f, 0.8f).normalized
            };

            bool anyDifferent = false;
            foreach (Vector3 direction in directions)
            {
                float first = SphericalHeightField.SampleHeight(direction, firstSettings);
                float second = SphericalHeightField.SampleHeight(direction, secondSettings);
                if (!Mathf.Approximately(first, second))
                {
                    anyDifferent = true;
                    break;
                }
            }

            Assert.IsTrue(anyDifferent, "Changing the seed should change at least one sampled height.");
        }

        [Test]
        public void HeightRemainsWithinConfiguredRange()
        {
            SphericalWorldSettings settings = new SphericalWorldSettings { maximumElevationMeters = 250f };
            for (int i = 0; i < 100; i++)
            {
                Vector3 direction = new Vector3(i + 1f, i * 0.37f + 1f, i * -0.19f + 2f).normalized;
                float height = SphericalHeightField.SampleHeight(direction, settings);
                Assert.That(height, Is.InRange(0f, settings.maximumElevationMeters));
            }
        }
    }
}
