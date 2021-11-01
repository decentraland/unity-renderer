using System;
using UnityEngine;

public class ExploreV2Bridge : MonoBehaviour
{
    public static event Action<string> OnDCLEventsReceived;

    public void DCLEvents(string payload) { OnDCLEventsReceived?.Invoke(payload); }
}