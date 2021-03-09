using System;
using System.Threading.Tasks;
using UnityEngine;

internal class ClipboardHandler_Mock : IClipboardHandler
{
    private Action<string, bool> OnRead;
    public string textInClipboard { private set; get; }

    private float delayForReadRequest = 0;
    private string errorInReadRequest = "";
    void IClipboardHandler.Initialize(Action<string, bool> onRead)
    {
        OnRead = onRead;
    }

    void IClipboardHandler.RequestWriteText(string text)
    {
        textInClipboard = text;
    }

    void IClipboardHandler.RequestGetText()
    {
        bool isError = !string.IsNullOrEmpty(errorInReadRequest);

        if (delayForReadRequest > 0)
        {
            Task.Delay(Mathf.FloorToInt(delayForReadRequest * 1000)).ContinueWith((task) =>
            {
                OnRead?.Invoke(isError? errorInReadRequest : textInClipboard, isError);
            });
        }
        else
        {
            OnRead?.Invoke(isError? errorInReadRequest : textInClipboard, isError);
        }
    }

    public void MockReadTextRequestResult(float delay, string resultValue, bool error)
    {
        textInClipboard = resultValue;
        delayForReadRequest = delay;
        errorInReadRequest = error ? resultValue : string.Empty;
    }
}
