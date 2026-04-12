namespace ClaudeProjectBackend.Application.Common.Interfaces;

/// <summary>
/// Generates unique, time-sortable 64-bit IDs (Snowflake-style).
/// Structure: 1 sign | 41 ms-timestamp | 10 machine | 12 sequence
/// </summary>
public interface ISnowflakeIdGenerator
{
    long NewId();
}
