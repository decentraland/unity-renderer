using System.Text.RegularExpressions;

public class CoordinateUtils
{
    private const string COORDINATE_MATCH_REGEX = @"(?<!\w)(?<!<\/noparse>)[-]?\d{1,3},[-]?\d{1,3}(?!\w)(?!<\/\/noparse>)";
    private const int MAX_X_COORDINATE = 163;
    private const int MIN_X_COORDINATE = -150;
    private const int MAX_Y_COORDINATE = 158;
    private const int MIN_Y_COORDINATE = -150;

    private static readonly Regex REGEX = new (COORDINATE_MATCH_REGEX, RegexOptions.IgnoreCase);

    public static bool IsCoordinateInRange(int x, int y) =>
        x is <= MAX_X_COORDINATE and >= MIN_X_COORDINATE && y is <= MAX_Y_COORDINATE and >= MIN_Y_COORDINATE;

    public delegate string CoordinatesReplacementDelegate(string matchedText, (int x, int y) coordinates);

    public static string ReplaceTextCoordinates(string text, CoordinatesReplacementDelegate replacementCallback)
    {
        return REGEX.Replace(text, match =>
        {
            string value = match.Value;
            int.TryParse(value.Split(',')[0], out int x);
            int.TryParse(value.Split(',')[1], out int y);

            return IsCoordinateInRange(x, y) ? replacementCallback.Invoke(value, (x, y)) : value;
        });
    }

    public static bool HasValidTextCoordinates(string text)
    {
        MatchCollection matches = REGEX.Matches(text);

        foreach (Match match in matches)
        {
            if (!match.Success) continue;
            string value = match.Value;
            int.TryParse(value.Split(',')[0], out int x);
            int.TryParse(value.Split(',')[1], out int y);

            if (IsCoordinateInRange(x, y))
                return true;
        }

        return false;
    }

    public static ParcelCoordinates ParseCoordinatesString(string coordinates) =>
        new (int.Parse(coordinates.Split(',')[0]), int.Parse(coordinates.Split(',')[1]));
}
