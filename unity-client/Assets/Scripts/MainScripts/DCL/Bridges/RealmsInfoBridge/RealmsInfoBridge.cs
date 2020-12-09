using UnityEngine;

public class RealmsInfoBridge : MonoBehaviour
{
    RealmsInfoHandler handler = new RealmsInfoHandler();

    public void UpdateRealmsInfo(string payload)
    {
        handler.Set(payload);
    }
}
