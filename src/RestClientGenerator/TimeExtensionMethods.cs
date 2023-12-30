namespace RestClient;

using System;

/// <summary>
/// Time extension methods.
/// </summary>
public static class TimeExtensionMethods
{
    /// <summary>
    /// A random generator.
    /// </summary>
    private static readonly Random RandomGenerator = new Random();

    /// <summary>
    /// Varies the <see cref="TimeSpan"/> by +/- 10 percent.
    /// </summary>
    /// <param name="timeSpan">A time span.</param>
    /// <returns>The varied time span.</returns>
    public static TimeSpan VaryTime(this TimeSpan timeSpan)
    {
        return timeSpan.VaryTime(0.1D);
    }

    /// <summary>
    /// Varies the <see cref="TimeSpan"/> by +/- the given percentage.
    /// </summary>
    /// <param name="timeSpan">A time span.</param>
    /// <param name="percentage">The percentage of time by which to vary the time. Valid values are bewteen 0.0 and 1.0.</param>
    /// <returns>The varied time span.</returns>
    public static TimeSpan VaryTime(this TimeSpan timeSpan, double percentage)
    {
        if (percentage < 0D || percentage > 1D)
        {
            throw new ArgumentOutOfRangeException(nameof(percentage));
        }

        var ms = timeSpan.TotalMilliseconds;
        var percentageMS = ms * percentage;
        return TimeSpan.FromMilliseconds(RandomGenerator.Next(Convert.ToInt32(ms - percentageMS), Convert.ToInt32(ms + percentageMS)));
    }

    /// <summary>
    /// Generate a <see cref="TimeSpan"/> with variance.
    /// </summary>
    /// <param name="timeSpan">The time span.</param>
    /// <param name="percentage">The percentage of time to vary. Valid values are bewteen 0.0 and 1.0.</param>
    /// <param name="baseTime">The base <see cref="TimeSpan"/> that the new <see cref="TimeSpan"/> cannot be lower than.</param>
    /// <returns>The new varied <see cref="TimeSpan"/>.</returns>
    public static TimeSpan VaryTime(this TimeSpan timeSpan, double percentage, TimeSpan baseTime)
    {
        var ms = timeSpan.TotalMilliseconds;
        var percentageMS = ms * percentage;
        return timeSpan.VaryTime(TimeSpan.FromMilliseconds(percentageMS), baseTime);
    }

    /// <summary>
    /// Generate a <see cref="TimeSpan"/> with variance.
    /// </summary>
    /// <param name="timeSpan">The time span.</param>
    /// <param name="variance">The amount of variance.</param>
    /// <param name="baseTime">The base <see cref="TimeSpan"/> that the new <see cref="TimeSpan"/> cannot be lower than.</param>
    /// <returns>The new varied <see cref="TimeSpan"/>.</returns>
    public static TimeSpan VaryTime(this TimeSpan timeSpan, TimeSpan variance, TimeSpan baseTime)
    {
        int timeSpanMS = (int)timeSpan.TotalMilliseconds;
        int varianceMS = (int)variance.TotalMilliseconds;
        int baseTimeMS = (int)baseTime.TotalMilliseconds;

        int waitLower = timeSpanMS - varianceMS > baseTimeMS ? timeSpanMS - varianceMS : Math.Max(timeSpanMS, baseTimeMS);
        int waitUpper = timeSpanMS + varianceMS;

        return TimeSpan.FromMilliseconds(RandomGenerator.Next(waitLower, waitUpper));
    }
}
