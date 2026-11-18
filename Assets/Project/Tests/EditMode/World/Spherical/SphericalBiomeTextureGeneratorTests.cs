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
        public void Generate_PlainsAndForestHaveDistinctSurfacePatterns()
        {
            const int resolution = 128;
            Color32[] plains = SphericalBiomeTextureGenerator.Generate(SphericalBiome.Plains, resolution, 481516);
            Color32[] forest = SphericalBiomeTextureGenerator.Generate(SphericalBiome.Forest, resolution, 481516);

            int differingPixels = 0;
            for (int i = 0; i < plains.Length; i++)
            {
                if (System.Math.Abs(plains[i].r - forest[i].r) > 3)
                    differingPixels++;
            }

            Assert.That(differingPixels, Is.GreaterThan(plains.Length / 3));
        }

        [Test]
        public void Generate_PlainsAndForestRemainOpaqueAndWithinNeutralMultiplierRange()
        {
            foreach (SphericalBiome biome in new[] { SphericalBiome.Plains, SphericalBiome.Forest })
            {
                Color32[] pixels = SphericalBiomeTextureGenerator.Generate(biome, 64, 99);
                foreach (Color32 pixel in pixels)
                {
                    Assert.AreEqual(255, pixel.a);
                    Assert.That(pixel.r, Is.InRange(173, 255));
                    Assert.That(pixel.g, Is.InRange(173, 255));
                    Assert.That(pixel.b, Is.InRange(173, 255));
                }
            }
        }

        [Test]
        public void Generate_PlainsContainsVisibleBladeVariation()
        {
            const int resolution = 128;
            Color32[] pixels = SphericalBiomeTextureGenerator.Generate(SphericalBiome.Plains, resolution, 481516);
            int visiblyVariedPixels = 0;
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 p = pixels[i];
                int spread = Mathf.Max(p.r, Mathf.Max(p.g, p.b)) - Mathf.Min(p.r, Mathf.Min(p.g, p.b));
                if (spread > 18) visiblyVariedPixels++;
            }

            Assert.That(visiblyVariedPixels, Is.GreaterThan(pixels.Length / 20));
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