using NUnit.Framework;
using Pokemon.Project.World.Spherical.Geography;
using Pokemon.Project.World.Spherical.Presentation;
using UnityEngine;

namespace Pokemon.Project.Tests.World.Spherical.EditMode
{
    public sealed class SphericalBiomeTextureGeneratorTests
    {
        [TestCase(SphericalBiome.Plains)]
        [TestCase(SphericalBiome.Forest)]
        [TestCase(SphericalBiome.Desert)]
        [TestCase(SphericalBiome.Mountains)]
        public void Generate_IsDeterministicAndReturnsOpaquePixels(SphericalBiome biome)
        {
            var first = SphericalBiomeTextureGenerator.Generate(biome, 64, 12345);
            var second = SphericalBiomeTextureGenerator.Generate(biome, 64, 12345);

            Assert.AreEqual(64 * 64, first.Length);
            Assert.AreEqual(first.Length, second.Length);

            for (int i = 0; i < first.Length; i++)
            {
                Assert.AreEqual(first[i], second[i]);
                Assert.AreEqual(255, first[i].a);
                Assert.That(first[i].r, Is.InRange(173, 255));
            }
        }

        [TestCase(SphericalBiome.Plains)]
        [TestCase(SphericalBiome.Forest)]
        [TestCase(SphericalBiome.Desert)]
        [TestCase(SphericalBiome.Mountains)]
        public void Generate_HasContinuousOppositeTextureEdges(SphericalBiome biome)
        {
            const int resolution = 128;

            var pixels = SphericalBiomeTextureGenerator.Generate(
                biome,
                resolution,
                77
            );

            for (int i = 0; i < resolution; i++)
            {
                Color32 left = pixels[i * resolution];
                Color32 right = pixels[i * resolution + resolution - 1];

                Assert.That(
                    System.Math.Abs(left.r - right.r),
                    Is.LessThan(40)
                );

                Color32 bottom = pixels[i];
                Color32 top = pixels[(resolution - 1) * resolution + i];

                Assert.That(
                    System.Math.Abs(bottom.r - top.r),
                    Is.LessThan(40)
                );
            }
        }

        [Test]
        public void Generate_RejectsWaterAsGroundTexture()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                SphericalBiomeTextureGenerator.Generate(
                    unchecked((SphericalBiome)999),
                    64,
                    1
                )
            );
        }
    }
}