using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using PingMonitor.Models;
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
    public RelayCommand ShowHistoryCommand { get; }

    private MonitorViewModel(IReadOnlyList<PingTargetOptions> targets, int intervalMs, MonitorPersistedState? initialState)
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
        ShowHistoryCommand = new RelayCommand(RequestHistory);

        RestoreFromState(initialState);

        foreach (var w in _workers)
            _ = w.RunAsync(_cts.Token);
    }

    public static MonitorViewModel CreateFromAppSettings(MonitorPersistedState? initialState = null)
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

        return new MonitorViewModel(new[] { router, internet }, intervalMs, initialState);
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


    private TargetStatsViewModel? FindTargetViewModel(MonitorTargetPersistedState targetState)
    {
        if (!string.IsNullOrWhiteSpace(targetState.Name)
            && _targetsByName.TryGetValue(targetState.Name, out var byName))
        {
            return byName;
        }

        if (!string.IsNullOrWhiteSpace(targetState.Host))
        {
            return Targets.FirstOrDefault(t =>
                string.Equals(t.Host, targetState.Host, StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }

    public MonitorPersistedState GetPersistedState()
    {
        var state = new MonitorPersistedState
        {
            IsRunning = IsRunning
        };

        foreach (var target in Targets)
        {
            var targetState = new MonitorTargetPersistedState
            {
                Name = target.Name,
                Host = target.Host,
                History = target.History
                    .Select(PingSampleState.FromSample)
                    .ToList()
            };

            state.Targets.Add(targetState);
        }

        return state;
    }

    private void RestoreFromState(MonitorPersistedState? state)
    {
        if (state is null)
            return;

        if (state.Targets is { Count: > 0 })
        {
            foreach (var targetState in state.Targets)
            {
                if (targetState is null)
                    continue;

                var vm = FindTargetViewModel(targetState);
                if (vm is null || targetState.History is not { Count: > 0 })
                    continue;

                foreach (var sampleState in targetState.History)
                    vm.AddSample(sampleState.ToSample());
            }
        }

        if (state.IsRunning is false && _isRunning)
            Toggle();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    public event EventHandler? ShowHistoryRequested;

    private void RequestHistory()
        => ShowHistoryRequested?.Invoke(this, EventArgs.Empty);

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
