using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Avalonia.Media;
using Avalonia.Platform;

namespace PingMonitor.Chrome;

internal static class SvgGeometryLoader
{
    private static readonly Regex PathDRegex =
        new("d\\s*=\\s*\"([^\"]+)\"", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static Geometry LoadOrFallback(string assetUri, string fallbackPathData)
    {
        try
        {
            using var stream = AssetLoader.Open(new Uri(assetUri));
            using var reader = new StreamReader(stream);
            var svg = reader.ReadToEnd();

            var parts = new List<string>();
            foreach (Match m in PathDRegex.Matches(svg))
            {
                var d = m.Groups[1].Value?.Trim();
                if (string.IsNullOrWhiteSpace(d))
                    continue;

                // Common tabler "no-op" bounding box path.
                if (string.Equals(d, "M0 0h24v24H0z", StringComparison.OrdinalIgnoreCase))
                    continue;

                parts.Add(d);
            }

            var combined = parts.Count == 0 ? fallbackPathData : string.Join(" ", parts);
            return Geometry.Parse(combined);
        }
        catch
        {
            return Geometry.Parse(fallbackPathData);
        }
    }
}
