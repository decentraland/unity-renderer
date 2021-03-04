using DCL;
using System;

public class MouseCatcher_Mock : IMouseCatcher
{
    public bool isLocked => false;

    public event Action OnMouseUnlock;
    public event Action OnMouseLock;

    public void RaiseMouseUnlock() { OnMouseUnlock?.Invoke(); }
    public void RaiseMouseLock() { OnMouseLock?.Invoke(); }
}
