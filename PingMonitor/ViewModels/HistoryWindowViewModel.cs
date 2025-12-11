using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PingMonitor.Models;

namespace PingMonitor.ViewModels;

public sealed class HistoryWindowViewModel
{
    public IReadOnlyList<PingHistoryNodeViewModel> Nodes { get; }

    private HistoryWindowViewModel(IReadOnlyList<PingHistoryNodeViewModel> nodes)
    {
        Nodes = nodes;
    }

    public static HistoryWindowViewModel CreateFromMonitor(MonitorViewModel monitor)
    {
        var nodes = new List<PingHistoryNodeViewModel>();

        foreach (var target in monitor.Targets)
        {
            var groups = BuildGroupsForTarget(target);
            var header = $"{target.Name} ({target.Host})";
            var description = $"{target.SummaryLine}  ·  {target.RangeLine}";
            nodes.Add(new PingHistoryNodeViewModel(header, description, groups));
        }

        return new HistoryWindowViewModel(nodes);
    }

    private static IReadOnlyList<PingHistoryNodeViewModel> BuildGroupsForTarget(TargetStatsViewModel target)
    {
        var samples = target.History;
        var result = new List<PingHistoryNodeViewModel>();

        if (samples.Count == 0)
            return result;

        string? currentState = null;
        var currentSamples = new List<PingSample>();

        foreach (var sample in samples)
        {
            var state = GetStateKey(sample);
            if (!string.Equals(state, currentState, StringComparison.Ordinal))
            {
                FlushGroup();
                currentState = state;
            }

            currentSamples.Add(sample);
        }

        FlushGroup();
        return result;

        void FlushGroup()
        {
            if (currentSamples.Count == 0)
                return;

            var start = currentSamples[0].Timestamp;
            var end = currentSamples[^1].Timestamp;
            if (end < start)
                end = start;

            var duration = end - start;
            var groupHeader = $"{currentState}  {FormatTime(start)} - {FormatTime(end)}  ({FormatDuration(duration)})  [{currentSamples.Count} samples]";
            var groupDescription = BuildGroupDescription(currentSamples);

            var children = new List<PingHistoryNodeViewModel>(currentSamples.Count);
            foreach (var sample in currentSamples)
            {
                var sampleHeader = $"{FormatTime(sample.Timestamp)}  {(sample.Success ? "OK" : "FAIL")}  {FormatRoundtrip(sample.RoundtripMs)}";
                var sampleDescription = BuildSampleDescription(sample);
                children.Add(new PingHistoryNodeViewModel(sampleHeader, sampleDescription, null));
            }

            result.Add(new PingHistoryNodeViewModel(groupHeader, groupDescription, children));
            currentSamples.Clear();
        }
    }

    private static string GetStateKey(PingSample sample)
        => sample.Success ? "Connected" : "Disconnected";

    private static string FormatTime(DateTimeOffset ts)
        => ts.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

    private static string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalSeconds < 1)
            return $"{duration.TotalMilliseconds:0} ms";
        if (duration.TotalMinutes < 1)
            return $"{duration.TotalSeconds:0.0} s";
        if (duration.TotalHours < 1)
            return $"{duration.TotalMinutes:0.0} min";
        return $"{duration.TotalHours:0.0} h";
    }

    private static string FormatRoundtrip(double? ms)
        => ms is null ? "—" : $"{ms.Value:0} ms";

    private static string BuildGroupDescription(IReadOnlyList<PingSample> samples)
    {
        if (samples.Count == 0)
            return string.Empty;

        var okCount = samples.Count(s => s.Success);
        var failCount = samples.Count - okCount;

        var text = $"OK {okCount}  ·  Fail {failCount}";

        if (okCount > 0)
        {
            var okSamples = samples.Where(s => s.Success && s.RoundtripMs is not null).ToList();
            if (okSamples.Count > 0)
            {
                var avg = okSamples.Average(s => s.RoundtripMs!.Value);
                var min = okSamples.Min(s => s.RoundtripMs!.Value);
                var max = okSamples.Max(s => s.RoundtripMs!.Value);
                text += $"  ·  Avg {avg:0} ms  ·  Min {min:0} ms  ·  Max {max:0} ms";
            }
        }

        if (failCount > 0)
        {
            var byError = samples
                .Where(s => !s.Success)
                .GroupBy(s => s.Error ?? s.ReplyStatus ?? s.ExceptionType ?? "Unknown")
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => $"{g.Key} x{g.Count()}");

            text += "  ·  Errors: " + string.Join(", ", byError);
        }

        return text;
    }

    private static string BuildSampleDescription(PingSample sample)
    {
        var parts = new List<string>
        {
            sample.Success ? "Status=OK" : "Status=Fail"
        };

        if (!string.IsNullOrEmpty(sample.Error))
            parts.Add($"Error={sample.Error}");

        if (!string.IsNullOrEmpty(sample.ReplyStatus) && !string.Equals(sample.ReplyStatus, sample.Error, StringComparison.Ordinal))
            parts.Add($"Reply={sample.ReplyStatus}");

        if (!string.IsNullOrEmpty(sample.ReplyAddress))
            parts.Add($"Address={sample.ReplyAddress}");

        if (sample.TimeoutMs is not null)
            parts.Add($"Timeout={sample.TimeoutMs} ms");

        if (!string.IsNullOrEmpty(sample.ExceptionType))
            parts.Add($"Exception={sample.ExceptionType}");

        if (!string.IsNullOrEmpty(sample.ExceptionMessage))
            parts.Add($"Message={sample.ExceptionMessage}");

        return string.Join("  ·  ", parts);
    }
}
