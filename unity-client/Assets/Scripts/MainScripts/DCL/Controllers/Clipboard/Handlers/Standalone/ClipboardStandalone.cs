using System;
using UnityEngine;

internal class ClipboardStandalone : IClipboardHandler
{
    private Action<string, bool> OnRead;

    void IClipboardHandler.Initialize(Action<string, bool> onRead)
    {
        this.OnRead = onRead;
    }

    void IClipboardHandler.RequestWriteText(string text)
    {
        GUIUtility.systemCopyBuffer = text;
    }

    void IClipboardHandler.RequestGetText()
    {
        OnRead?.Invoke(GUIUtility.systemCopyBuffer, false);
    }
}