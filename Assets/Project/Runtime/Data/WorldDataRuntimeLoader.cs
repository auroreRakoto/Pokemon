using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Pokemon.Project.World.Environment;

namespace Pokemon.Project.Data
{
    /// <summary>
    /// Loads canonical JSON and numeric map grids copied to StreamingAssets/PokemonData.
    /// This class intentionally knows nothing about terrain rendering or biome rules.
    /// </summary>
    public static class WorldDataRuntimeLoader
    {
        private const string DataFolder = "PokemonData";

        public static IEnumerator LoadJson<T>(string relativePath, Action<T> onSuccess, Action<string> onError)
            where T : class
        {
            if (!TryBuildUri(relativePath, out string uri, out string error))
            {
                onError?.Invoke(error);
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(uri))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke($"JSON load failed ({relativePath}): {request.error}");
                    yield break;
                }

                if (!TryParseJson(request.downloadHandler.text, out T data, out string parseError))
                {
                    onError?.Invoke($"JSON parse failed ({relativePath}): {parseError}");
                    yield break;
                }
                onSuccess?.Invoke(data);
            }
        }

        public static IEnumerator LoadGrid(string manifestRelativePath,
            Action<EnvironmentGrid> onSuccess, Action<string> onError)
        {
            MapLayerManifestData manifest = null;
            string loadError = null;
            yield return LoadJson<MapLayerManifestData>(manifestRelativePath,
                value => manifest = value,
                error => loadError = error);

            if (!string.IsNullOrEmpty(loadError))
            {
                onError?.Invoke(loadError);
                yield break;
            }

            if (!ValidateManifest(manifest, out string validationError))
            {
                onError?.Invoke($"Invalid map manifest ({manifestRelativePath}): {validationError}");
                yield break;
            }

            string dataRelativePath = CombineRelative(Path.GetDirectoryName(manifestRelativePath), manifest.dataFile);
            if (!TryBuildUri(dataRelativePath, out string dataUri, out string uriError))
            {
                onError?.Invoke(uriError);
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(dataUri))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke($"Grid load failed ({dataRelativePath}): {request.error}");
                    yield break;
                }

                byte[] bytes = request.downloadHandler.data;
                int expectedBytes = checked(manifest.width * manifest.height * sizeof(float));
                if (bytes == null || bytes.Length != expectedBytes)
                {
                    onError?.Invoke($"Grid byte length mismatch. Expected {expectedBytes}, received {bytes?.Length ?? 0}.");
                    yield break;
                }

                if (!TryDecodeGrid(bytes, manifest, out float[] values, out string decodeError))
                {
                    onError?.Invoke($"Grid decode failed: {decodeError}");
                    yield break;
                }
                onSuccess?.Invoke(new EnvironmentGrid(manifest, values));
            }
        }


        private static bool TryParseJson<T>(string json, out T data, out string error) where T : class
        {
            try
            {
                data = JsonUtility.FromJson<T>(json);
                error = data == null ? "Deserialization returned null." : null;
                return data != null;
            }
            catch (Exception exception)
            {
                data = null;
                error = exception.Message;
                return false;
            }
        }

        private static bool TryDecodeGrid(byte[] bytes, MapLayerManifestData manifest, out float[] values, out string error)
        {
            values = null;
            error = null;
            try
            {
                values = new float[manifest.width * manifest.height];
                for (int i = 0; i < values.Length; i++)
                {
                    int offset = i * sizeof(float);
                    if (BitConverter.IsLittleEndian)
                    {
                        values[i] = BitConverter.ToSingle(bytes, offset);
                    }
                    else
                    {
                        byte[] reversed = { bytes[offset + 3], bytes[offset + 2], bytes[offset + 1], bytes[offset] };
                        values[i] = BitConverter.ToSingle(reversed, 0);
                    }

                    if (float.IsNaN(values[i]) || float.IsInfinity(values[i]) ||
                        values[i] < manifest.valueRange.min - 0.000001f ||
                        values[i] > manifest.valueRange.max + 0.000001f)
                    {
                        error = $"Invalid grid value at index {i}: {values[i]}";
                        values = null;
                        return false;
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                values = null;
                error = exception.Message;
                return false;
            }
        }

        private static bool ValidateManifest(MapLayerManifestData manifest, out string error)
        {
            if (manifest == null) { error = "Manifest is null."; return false; }
            if (manifest.schemaVersion != 1) { error = "Unsupported schemaVersion."; return false; }
            if (manifest.encoding != "float32" || manifest.byteOrder != "little" || manifest.channels != 1)
            { error = "Only float32 little-endian single-channel grids are supported by this first loader."; return false; }
            if (manifest.width < 2 || manifest.height < 2) { error = "Grid dimensions must be >= 2."; return false; }
            if (manifest.valueRange == null || manifest.valueRange.min > manifest.valueRange.max)
            { error = "Invalid valueRange."; return false; }
            if (manifest.origin == null || manifest.worldSize == null || manifest.worldSize.x <= 0f || manifest.worldSize.z <= 0f)
            { error = "Invalid world coordinates."; return false; }
            if (string.IsNullOrWhiteSpace(manifest.dataFile) || Path.IsPathRooted(manifest.dataFile) || manifest.dataFile.Contains(".."))
            { error = "dataFile must be a safe relative path."; return false; }
            if (manifest.rowOrder != "south_to_north") { error = "Unsupported rowOrder."; return false; }
            error = null;
            return true;
        }

        private static string CombineRelative(string folder, string file)
        {
            string combined = string.IsNullOrEmpty(folder) ? file : folder.TrimEnd('/', '\\') + "/" + file;
            return combined.Replace('\\', '/').TrimStart('/');
        }

        private static bool TryBuildUri(string relativePath, out string uri, out string error)
        {
            uri = null;
            error = null;
            if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath) || relativePath.Contains(".."))
            {
                error = $"Expected a safe relative data path, received: {relativePath}";
                return false;
            }

            string root = Application.streamingAssetsPath.TrimEnd('/', '\\');
            string path = root + "/" + DataFolder + "/" + relativePath.Replace('\\', '/');

            // Android/WebGL may expose StreamingAssets as a URL rather than a filesystem path.
            if (path.StartsWith("jar:", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                uri = path;
                return true;
            }

            try
            {
                uri = new Uri(Path.GetFullPath(path)).AbsoluteUri;
                return true;
            }
            catch (Exception exception)
            {
                error = $"Could not construct StreamingAssets URI: {exception.Message}";
                return false;
            }
        }
    }
}
