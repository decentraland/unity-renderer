using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CoordinateUtils
{
    public const string COORDINATE_MATCH_REGEX = "[-]?\\d{1,3},[-]?\\d{1,3}";
    public const int MAX_X_COORDINATE = 163;
    public const int MIN_X_COORDINATE = -150;
    public const int MAX_Y_COORDINATE = 158;
    public const int MIN_Y_COORDINATE = -150;

    public static bool IsCoordinateInRange(int x, int y)
    {
        return x <= MAX_X_COORDINATE && x >= MIN_X_COORDINATE && y <= MAX_Y_COORDINATE && y >= MIN_Y_COORDINATE;
    }

    public static List<string> GetTextCoordinates(string text)
    {
        Regex filter = new Regex(COORDINATE_MATCH_REGEX);
        List<string> matchingWords = new List<string>();
        foreach (var item in text.Split(' '))
        {
            var match = filter.Match(item);
            int x;
            int y;
            if (match.Success)
            {
                Int32.TryParse(item.Split(',')[0], out x);
                Int32.TryParse(item.Split(',')[1], out y);

                if (IsCoordinateInRange(x, y))
                    matchingWords.Add(match.Value);
            }
        }

        return matchingWords;
    }

    public static bool HasValidTextCoordinates(string text)
    {
        return GetTextCoordinates(text).Count > 0;
    }

    public static ParcelCoordinates ParseCoordinatesString(string coordinates)
    {
        return new ParcelCoordinates(Int32.Parse(coordinates.Split(',')[0]), Int32.Parse(coordinates.Split(',')[1]));
    }
}