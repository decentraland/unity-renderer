using System.Collections.Generic;
using UnityEngine;

public class SimpleOverlappingTracker
{
    /// <summary>
    /// How much overlapping is allowed
    /// </summary>
    internal readonly float sqrTooCloseDistance;
    internal readonly List<Vector2> positions = new List<Vector2>();

    public SimpleOverlappingTracker(float tooCloseDistance) { sqrTooCloseDistance = tooCloseDistance * tooCloseDistance; }

    public void Reset() { positions.Clear(); }

    /// <summary>
    /// Add a position to the cluster, returns true if the position is allowed
    /// </summary>
    /// <returns></returns>
    public bool RegisterPosition (Vector2 position)
    {
        for (var i = 0; i < positions.Count; i++)
        {
            Vector2 toCompare = positions[i];
            if ((toCompare - position).sqrMagnitude < sqrTooCloseDistance)
                return false;
        }
        positions.Add(position);
        return true;
    }
}