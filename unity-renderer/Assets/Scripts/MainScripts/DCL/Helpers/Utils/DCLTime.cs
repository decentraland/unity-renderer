using System.Diagnostics;
using UnityEngine;

public static class DCLTime
{
    static DCLTime()
    {
    }

    public static float realtimeSinceStartup
    {
        get
        {
            return Time.realtimeSinceStartup;
        }
    }
}