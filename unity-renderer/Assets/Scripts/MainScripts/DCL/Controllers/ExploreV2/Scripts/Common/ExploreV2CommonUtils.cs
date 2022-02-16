/// <summary>
/// Utils related to the events management in ExploreV2.
/// </summary>
public static class ExploreV2CommonUtils
{
    /// <summary>
    /// Converts a number to string using up to 2 decimals if needed.
    /// </summary>
    /// <param name="numberToFormat">Number to convert.</param>
    /// <returns>A formatted string.</returns>
    public static string FormatNumberToString(int numberToFormat)
    {
        float dividedNumber = numberToFormat >= 1000 ? (numberToFormat / 1000f) : numberToFormat;
        string result = numberToFormat >= 1000 ? $"{string.Format("{0:0.##}", dividedNumber)}k" : $"{dividedNumber}";

        return result;
    }
}
