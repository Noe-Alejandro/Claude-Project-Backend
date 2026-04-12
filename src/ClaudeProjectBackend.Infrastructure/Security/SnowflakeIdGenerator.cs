using ClaudeProjectBackend.Application.Common.Interfaces;

namespace ClaudeProjectBackend.Infrastructure.Security;

/// <summary>
/// Thread-safe Snowflake ID generator — produces unique, time-sortable 64-bit longs.
///
/// Bit layout (same concept as Twitter Snowflake / Steam SteamID):
///   Bit 63     : sign — always 0 (positive)
///   Bits 22-62 : milliseconds since custom epoch (41 bits → ~69 years)
///   Bits 12-21 : machine ID                      (10 bits → 0-1023)
///   Bits  0-11 : per-millisecond sequence         (12 bits → 0-4095)
///
/// Example output: 375,296,004,144,381,953
///   → sortable, non-sequential externally, no DB round-trip needed.
/// </summary>
public sealed class SnowflakeIdGenerator : ISnowflakeIdGenerator
{
    // Custom epoch: 2024-01-01 00:00:00 UTC  (~69 years of IDs from this date)
    private static readonly DateTimeOffset Epoch = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private const int MachineId = 1;        // Change per server instance (0-1023)
    private const int MachineBits = 10;
    private const int SequenceBits = 12;
    private const long MaxSequence = (1L << SequenceBits) - 1; // 4095

    private readonly object _sync = new();
    private long _lastTimestamp = -1;
    private long _sequence = 0;

    public long NewId()
    {
        lock (_sync)
        {
            var timestamp = GetCurrentMs();

            if (timestamp == _lastTimestamp)
            {
                _sequence = (_sequence + 1) & MaxSequence;
                if (_sequence == 0)
                    timestamp = WaitNextMs(_lastTimestamp); // sequence exhausted — wait 1ms
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = timestamp;

            return (timestamp << (MachineBits + SequenceBits))
                 | ((long)MachineId << SequenceBits)
                 | _sequence;
        }
    }

    private static long GetCurrentMs() =>
        (long)(DateTimeOffset.UtcNow - Epoch).TotalMilliseconds;

    private static long WaitNextMs(long lastMs)
    {
        var ts = GetCurrentMs();
        while (ts <= lastMs) ts = GetCurrentMs();
        return ts;
    }
}
