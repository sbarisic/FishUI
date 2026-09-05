using System;

namespace FishUI;

public interface IFishUINumericRange
{
    float MinValue { get; set; }
    float MaxValue { get; set; }
    float Value { get; set; }
    void SetRange(float minimum, float maximum);
}

internal static class NumericRange
{
    [ThreadStatic] internal static bool ReadingLayout;

    internal static float Finite(float value)
    {
        if (!float.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value), "A finite value is required.");
        return value;
    }

    internal static float NonNegative(float value)
    {
        if (Finite(value) < 0) throw new ArgumentOutOfRangeException(nameof(value));
        return value;
    }

    internal static void Validate(float minimum, float maximum)
    {
        Finite(minimum);
        Finite(maximum);
        if (minimum > maximum) throw new ArgumentOutOfRangeException(nameof(minimum), "Minimum must not exceed maximum.");
    }
}
