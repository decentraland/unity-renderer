using System;
using System.Runtime.InteropServices;
using AOT;
using DCL;
using UnityEngine;

internal class ClipboardWebGL : Singleton<ClipboardWebGL>, IClipboardHandler, IDisposable
{
    private Action<string, bool> OnRead;
    private bool copyInput = false;

    private delegate void ReadTextCallback(IntPtr ptrText, int intError);

    private delegate void OnPasteInputCallback(IntPtr ptrText);

    private delegate void OnCopyInputCallback();

    [DllImport("__Internal")]
    private static extern void initialize(Action<IntPtr, int> readTextCallback, Action<IntPtr> pasteCallback,
        Action copyCallback);

    /// <summary>
    /// External call to write text in the browser's clipboard
    /// </summary>
    /// <param name="text">string to push to the clipboard</param>
    [DllImport("__Internal")]
    private static extern void writeText(string text);

    /// <summary>
    /// External call to request the string value stored at browser's clipboard
    /// </summary>
    [DllImport("__Internal")]
    private static extern void readText();

    /// <summary>
    /// This static function is called from the browser. It will receive a pointer to the string value
    /// stored at browser's clipboard or and error if it couldn't get the value
    /// </summary>
    /// <param name="ptrText">pointer to the clipboard's string value</param>
    /// <param name="intError">0 if error, other if OK</param>
    [MonoPInvokeCallback(typeof(ReadTextCallback))]
    private static void OnReceiveReadText(IntPtr ptrText, int intError)
    {
        string value = Marshal.PtrToStringAuto(ptrText);
        bool error = intError == 0;
        i?.OnRead?.Invoke(value, error);
    }

    /// <summary>
    /// This static function is called from the browser. It will be called when a PASTE input is performed (CTRL+V)
    /// and it will receive a pointer to the string value stored at browser's clipboard
    /// </summary>
    /// <param name="ptrText">pointer to the clipboard's string value</param>
    [MonoPInvokeCallback(typeof(OnPasteInputCallback))]
    private static void OnReceivePasteInput(IntPtr ptrText)
    {
        string value = Marshal.PtrToStringAuto(ptrText);
        // NOTE: after marshalling we overwrite unity's clipboard buffer with the value coming from the browser
        GUIUtility.systemCopyBuffer = value;
    }

    /// <summary>
    /// This static function is called from the browser. It will be called when a COPY input is performed (CTRL+C)
    /// </summary>
    [MonoPInvokeCallback(typeof(OnCopyInputCallback))]
    private static void OnReceiveCopyInput()
    {
        // NOTE: here we set the flag that a copy input was performed to be used in OnBeforeRender function
        if (i != null) i.copyInput = true;
    }

    public ClipboardWebGL()
    {
        Application.onBeforeRender += OnBeforeRender;
    }

    public void Dispose()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }

    void IClipboardHandler.Initialize(Action<string, bool> onRead)
    {
        this.OnRead = onRead;
        initialize(OnReceiveReadText, OnReceivePasteInput, OnReceiveCopyInput);
    }

    void IClipboardHandler.RequestWriteText(string text)
    {
        writeText(text);
    }

    void IClipboardHandler.RequestGetText()
    {
        readText();
    }

    void OnBeforeRender()
    {
        // NOTE: before rendering (just after Unity's input is processed) we check if there was a COPY input (CTRL+C) triggered by
        // the browser. If there was a COPY input we push the text copied and stored inside Unity's clipboard into the browser's clipboard.
        // It is done this way cause we don't have a callback for Unity's copy input and because we want to store the value in
        // browser's clipboard so we are able to paste it outside Unity's "sandboxing"
        if (copyInput)
        {
            copyInput = false;
            writeText(GUIUtility.systemCopyBuffer);
        }
    }
}