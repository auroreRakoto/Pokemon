
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central point for coordinate-space translations.
///
/// If no conversion is registered, the input coordinates are returned
/// unchanged. This allows every module to use the translator,
/// even when no conversion is currently required.
/// </summary>
public sealed class CoordTranslator
{
    public enum CoordinateSpace
    {
        Grid2D,
        World2D,
        World3D,
        SphericalWorld
    }

    private readonly Dictionary<int, Matrix4x4> conversions =
        new Dictionary<int, Matrix4x4>();

    /// <summary>
    /// Registers a conversion from one coordinate space to another.
    /// The matrix transforms a position from source space into target space.
    /// </summary>
    public void RegisterConversion(
        CoordinateSpace source,
        CoordinateSpace target,
        Matrix4x4 conversionMatrix)
    {
        if (source == target)
            throw new ArgumentException(
                "Source and target coordinate spaces must be different.");

        int key = GetKey(source, target);

        conversions[key] = conversionMatrix;
    }

    /// <summary>
    /// Translates coordinates between two spaces.
    /// Returns the original coordinates when no conversion is registered.
    /// </summary>
    public Vector3 Translate(
        Vector3 coordinates,
        CoordinateSpace source,
        CoordinateSpace target)
    {
        if (source == target)
            return coordinates;

        int key = GetKey(source, target);

        if (!conversions.TryGetValue(key, out Matrix4x4 matrix))
            return coordinates;

        return matrix.MultiplyPoint3x4(coordinates);
    }

    /// <summary>
    /// Checks whether an explicit conversion exists.
    /// </summary>
    public bool HasConversion(
        CoordinateSpace source,
        CoordinateSpace target)
    {
        if (source == target)
            return true;

        return conversions.ContainsKey(GetKey(source, target));
    }

    /// <summary>
    /// Removes a registered conversion.
    /// </summary>
    public void RemoveConversion(
        CoordinateSpace source,
        CoordinateSpace target)
    {
        conversions.Remove(GetKey(source, target));
    }

    private int GetKey(
        CoordinateSpace source,
        CoordinateSpace target)
    {
        return ((int)source * Enum.GetValues(
            typeof(CoordinateSpace)).Length) + (int)target;
    }
}
