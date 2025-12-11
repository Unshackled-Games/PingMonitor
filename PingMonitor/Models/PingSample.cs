using System;

namespace PingMonitor.Models;

public sealed record PingSample(
    string TargetName,
    string Host,
    DateTimeOffset Timestamp,
    bool Success,
    double? RoundtripMs,
    string? Error,
    string? ReplyStatus,
    string? ReplyAddress,
    int? TimeoutMs,
    string? ExceptionType,
    string? ExceptionMessage
);
