using UnityEngine;

public class HUDAudioHandler : MonoBehaviour
{
    public static HUDAudioHandler i { get; private set; }

    [HideInInspector]
    public ulong chatLastCheckedTimestamp;

    private void Awake()
    {
        if (i != null)
        {
            Destroy(this);
            return;
        }

        i = this;

        RefreshChatLastCheckedTimestamp();
    }

    public void RefreshChatLastCheckedTimestamp()
    {
        // Get UTC datetime (used to determine whether chat messages are old or new)
        chatLastCheckedTimestamp = (ulong)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalMilliseconds;
    }
}