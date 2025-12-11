using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using PingMonitor.Models;

namespace PingMonitor.Services;

public sealed class PingWorker
{
    private readonly PingTargetOptions _opt;
    private readonly Action<PingSample> _onSample;
    private volatile bool _enabled = true;

    public PingWorker(PingTargetOptions opt, Action<PingSample> onSample)
    {
        _opt = opt;
        _onSample = onSample;
    }

    public void SetEnabled(bool enabled) => _enabled = enabled;

    public async Task RunAsync(CancellationToken token)
    {
        using var ping = new Ping();

        while (!token.IsCancellationRequested)
        {
            if (!_enabled)
            {
                await Task.Delay(150, token).ConfigureAwait(false);
                continue;
            }

            var started = DateTimeOffset.UtcNow;

            try
            {
                var reply = await ping.SendPingAsync(_opt.Host, _opt.TimeoutMs).ConfigureAwait(false);

                if (reply.Status == IPStatus.Success)
                {
                    _onSample(new PingSample(
                        TargetName: _opt.Name,
                        Host: _opt.Host,
                        Timestamp: DateTimeOffset.Now,
                        Success: true,
                        RoundtripMs: reply.RoundtripTime,
                        Error: null
                    ));
                }
                else
                {
                    _onSample(new PingSample(
                        TargetName: _opt.Name,
                        Host: _opt.Host,
                        Timestamp: DateTimeOffset.Now,
                        Success: false,
                        RoundtripMs: null,
                        Error: reply.Status.ToString()
                    ));
                }
            }
            catch (Exception ex) when (!token.IsCancellationRequested)
            {
                _onSample(new PingSample(
                    TargetName: _opt.Name,
                    Host: _opt.Host,
                    Timestamp: DateTimeOffset.Now,
                    Success: false,
                    RoundtripMs: null,
                    Error: ex.GetType().Name
                ));
            }

            var elapsedMs = (int)Math.Max(0, (DateTimeOffset.UtcNow - started).TotalMilliseconds);
            var delayMs = Math.Max(50, _opt.IntervalMs - elapsedMs);
            await Task.Delay(delayMs, token).ConfigureAwait(false);
        }
    }
}
