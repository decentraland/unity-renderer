using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForSecondsCache
{
    private static Dictionary<string, WaitForSeconds> waitCache = new Dictionary<string, WaitForSeconds>();

    public static WaitForSeconds Get(float seconds)
    {
        string key = seconds.ToString("0.0");

        if (!waitCache.ContainsKey(key))
        {
            float rounded = (float)Math.Round(seconds, 1);
            waitCache.Add(key, new WaitForSeconds(rounded));
        }

        return waitCache[key];
    }
}