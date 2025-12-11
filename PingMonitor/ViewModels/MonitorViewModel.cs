using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using PingMonitor.Services;

namespace PingMonitor.ViewModels;

public sealed class MonitorViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly PingWorker[] _workers;
    private readonly Dictionary<string, TargetStatsViewModel> _targetsByName;

    private bool _isRunning = true;
    public bool IsRunning
    {
        get => _isRunning;
        private set { _isRunning = value; OnPropertyChanged(); OnPropertyChanged(nameof(Subtitle)); }
    }

    public int IntervalMs { get; }
    public string Subtitle => IsRunning ? "Running" : "Paused";

    public IReadOnlyList<TargetStatsViewModel> Targets { get; }

    public RelayCommand ToggleCommand { get; }

    private MonitorViewModel(IReadOnlyList<PingTargetOptions> targets, int intervalMs)
    {
        IntervalMs = intervalMs;

        var vms = new List<TargetStatsViewModel>(targets.Count);
        _targetsByName = new Dictionary<string, TargetStatsViewModel>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in targets)
        {
            var vm = new TargetStatsViewModel(t.Name, t.Host);
            vms.Add(vm);
            _targetsByName[t.Name] = vm;
        }

        Targets = vms;

        var workers = new List<PingWorker>(targets.Count);
        foreach (var t in targets)
            workers.Add(new PingWorker(t, OnSample));

        _workers = workers.ToArray();

        ToggleCommand = new RelayCommand(Toggle);

        foreach (var w in _workers)
            _ = w.RunAsync(_cts.Token);
    }

    public static MonitorViewModel CreateFromAppSettings()
    {
        var cfg = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var router = ReadTarget(cfg, "Targets:Router", defaultName: "Router", defaultHost: "192.168.178.1");
        var internet = ReadTarget(cfg, "Targets:Internet", defaultName: "Internet", defaultHost: "1.1.1.1");

        var intervalMs = cfg.GetValue("Ping:IntervalMs", 1000);
        var timeoutMs = cfg.GetValue("Ping:TimeoutMs", 1000);

        router = router with { IntervalMs = intervalMs, TimeoutMs = timeoutMs };
        internet = internet with { IntervalMs = intervalMs, TimeoutMs = timeoutMs };

        return new MonitorViewModel(new[] { router, internet }, intervalMs);
    }

    private static PingTargetOptions ReadTarget(IConfiguration cfg, string prefix, string defaultName, string defaultHost)
    {
        var name = cfg.GetValue($"{prefix}:Name", defaultName);
        var host = cfg.GetValue($"{prefix}:Host", defaultHost);

        return new PingTargetOptions(
            Name: name,
            Host: host,
            IntervalMs: 1000,
            TimeoutMs: 1000
        );
    }

    private void Toggle()
    {
        IsRunning = !IsRunning;
        foreach (var w in _workers)
            w.SetEnabled(IsRunning);
    }

    private void OnSample(Models.PingSample sample)
        => Dispatcher.UIThread.Post(() =>
        {
            if (_targetsByName.TryGetValue(sample.TargetName, out var target))
                target.AddSample(sample);
        });

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
