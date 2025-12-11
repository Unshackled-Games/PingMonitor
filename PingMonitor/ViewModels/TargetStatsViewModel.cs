using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using PingMonitor.Models;

namespace PingMonitor.ViewModels;

public sealed class TargetStatsViewModel : INotifyPropertyChanged
{
    private const int MaxHistorySamples = 10_000;
    
    public string Name { get; }
    public string Host { get; }

    private long _total;
    private long _success;
    private double _sumMs;
    private double? _minMs;
    private double? _maxMs;
    private DateTimeOffset? _lastAt;
    private readonly List<PingSample> _history = new(MaxHistorySamples + 1);
    
    private string _currentDisplay = "—";
    private string _status = "No data";

    public string CurrentDisplay { get => _currentDisplay; private set { _currentDisplay = value; OnPropertyChanged(); } }
    public string Status { get => _status; private set { _status = value; OnPropertyChanged(); } }

    public string AverageDisplay => _success == 0 ? "—" : $"{(_sumMs / _success):0} ms";
    public string LossDisplay => _total == 0 ? "—" : $"{(100.0 * (_total - _success) / _total):0.0}%";
    public string MinDisplay => _minMs is null ? "—" : $"{_minMs:0} ms";
    public string MaxDisplay => _maxMs is null ? "—" : $"{_maxMs:0} ms";
    public string CountsDisplay => $"Samples: {_total}   Success: {_success}   Fail: {_total - _success}";
    public string SummaryLine => $"Avg {AverageDisplay}  ·  Loss {LossDisplay}";
    public string RangeLine => $"Min {MinDisplay}  ·  Max {MaxDisplay}";
    public IReadOnlyList<PingSample> History => _history;
    
    public string LastUpdatedDisplay
        => _lastAt is null ? "—" : _lastAt.Value.LocalDateTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

    public TargetStatsViewModel(string name, string host)
    {
        Name = name;
        Host = host;
    }

    public void AddSample(PingSample sample)
    {
        AppendHistory(sample);
        _total++;
        _lastAt = sample.Timestamp;

        if (sample.Success && sample.RoundtripMs is { } ms)
        {
            _success++;
            _sumMs += ms;
            _minMs = _minMs is null ? ms : Math.Min(_minMs.Value, ms);
            _maxMs = _maxMs is null ? ms : Math.Max(_maxMs.Value, ms);

            CurrentDisplay = $"{ms:0} ms";
            Status = "OK";
        }
        else
        {
            CurrentDisplay = "—";
            Status = sample.Error ?? "Fail";
        }

        OnPropertyChanged(nameof(AverageDisplay));
        OnPropertyChanged(nameof(LossDisplay));
        OnPropertyChanged(nameof(MinDisplay));
        OnPropertyChanged(nameof(MaxDisplay));
        OnPropertyChanged(nameof(CountsDisplay));
        OnPropertyChanged(nameof(SummaryLine));
        OnPropertyChanged(nameof(RangeLine));
        OnPropertyChanged(nameof(LastUpdatedDisplay));
    }
    
    private void AppendHistory(PingSample sample)
    {
        _history.Add(sample);
        if (_history.Count > MaxHistorySamples)
            _history.RemoveAt(0);
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
