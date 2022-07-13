using System;

internal interface IAnchorPointsGetterHandler : IDisposable
{
    event Action<string> OnAvatarRemoved;
    event Action<string, IAvatarAnchorPoints> OnAvatarFound;
    void GetAnchorPoints(string id);
    void CleanUp();
}