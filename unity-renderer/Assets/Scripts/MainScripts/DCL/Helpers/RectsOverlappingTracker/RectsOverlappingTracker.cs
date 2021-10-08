using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This utility class tracks rect and avoid overlaps under certain tolerance
/// </summary>
public class RectsOverlappingTracker
{
    /// <summary>
    /// How much overlapping is allowed
    /// </summary>
    internal readonly float overlappingTolerance;
    internal readonly List<Rect> rects = new List<Rect>();

    public RectsOverlappingTracker(float overlappingTolerance) { this.overlappingTolerance = Mathf.Clamp01(overlappingTolerance); }

    public void Reset() { rects.Clear(); }

    /// <summary>
    /// Add a rect to the cluster skipper, returns if the rect is colliding with another
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public bool RegisterRect(Rect rect)
    {
        // Alex: This algorithm scales badly with the number of rects.
        // To reduce the impact on performance the heuristic is very aggresive,
        // is going to add together all the overlapping areas found (and ignore if these areas overlap eachother).
        // This will cause false positives where a rect overlapped under the threshold value won't be registered
        // but since the registering should be made in priority order (i.e. by distance), it should be okay.

        float initialArea = rect.size.x * rect.size.y;
        float currentArea = 0;
        for (var i = 0; i < rects.Count; i++)
        {
            Rect toCompare = rects[i];
            if (!TryGetIntersectionArea(toCompare, rect, out float area))
                continue;
            currentArea += area;
            if ( (currentArea / initialArea) >=  overlappingTolerance)
                return false;
        }
        rects.Add(rect);
        return true;
    }

    internal static bool TryGetIntersectionArea(Rect r1, Rect r2, out float area)
    {
        area = -1;

        float x1 = Mathf.Min(r1.xMax, r2.xMax);
        float x2 = Mathf.Max(r1.xMin, r2.xMin);
        float y1 = Mathf.Min(r1.yMax, r2.yMax);
        float y2 = Mathf.Max(r1.yMin, r2.yMin);

        if (x1 <= x2 || y1 <= y2 ) // Not overlapping (way faster than Rect.Overlap)
            return false;

        area = Mathf.Abs((x1 - x2) * (y1 - y2));
        return true;
    }
}