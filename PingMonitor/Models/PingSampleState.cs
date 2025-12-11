using System;

namespace PingMonitor.Models;

public sealed class PingSampleState
{
    public string? TargetName { get; set; }
    public string? Host { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public bool Success { get; set; }
    public double? RoundtripMs { get; set; }
    public string? Error { get; set; }
    public string? ReplyStatus { get; set; }
    public string? ReplyAddress { get; set; }
    public int? TimeoutMs { get; set; }
    public string? ExceptionType { get; set; }
    public string? ExceptionMessage { get; set; }

    public PingSample ToSample()
    {
        return new PingSample(
            TargetName ?? string.Empty,
            Host ?? string.Empty,
            Timestamp == default ? DateTimeOffset.Now : Timestamp,
            Success,
            RoundtripMs,
            Error,
            ReplyStatus,
            ReplyAddress,
            TimeoutMs,
            ExceptionType,
            ExceptionMessage
        );
    }

    public static PingSampleState FromSample(PingSample sample)
    {
        return new PingSampleState
        {
            TargetName = sample.TargetName,
            Host = sample.Host,
            Timestamp = sample.Timestamp,
            Success = sample.Success,
            RoundtripMs = sample.RoundtripMs,
            Error = sample.Error,
            ReplyStatus = sample.ReplyStatus,
            ReplyAddress = sample.ReplyAddress,
            TimeoutMs = sample.TimeoutMs,
            ExceptionType = sample.ExceptionType,
            ExceptionMessage = sample.ExceptionMessage
        };
    } 
}
