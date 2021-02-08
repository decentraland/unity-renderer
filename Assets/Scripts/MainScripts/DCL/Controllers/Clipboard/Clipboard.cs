using System;
using System.Collections.Generic;
using DCL.Helpers;

public interface IClipboard
{
    /// <summary>
    /// Push a string value to the clipboard
    /// </summary>
    /// <param name="text">string to store</param>
    void WriteText(string text);

    /// <summary>
    /// Request the string stored at the clipboard
    /// </summary>
    /// <returns>Promise of the string value stored at clipboard</returns>
    Promise<string> ReadText();
}

public class Clipboard : IClipboard
{
    private readonly Queue<Promise<string>> promises = new Queue<Promise<string>>();
    private readonly IClipboardHandler handler = null;

    /// <summary>
    /// Create a platform specific instance of Clipboard
    /// </summary>
    /// <returns>Clipboard instance</returns>
    public static Clipboard Create()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return new Clipboard(ClipboardWebGL.i);
#else
        return new Clipboard(new ClipboardStandalone());
#endif
    }

    /// <summary>
    /// Push a string value to the clipboard
    /// </summary>
    /// <param name="text">string to store</param>
    public void WriteText(string text)
    {
        handler?.RequestWriteText(text);
    }

    /// <summary>
    /// Request the string stored at the clipboard
    /// </summary>
    /// <returns>Promise of the string value stored at clipboard</returns>
    [Obsolete("Firefox not supported")]
    public Promise<string> ReadText()
    {
        Promise<string> promise = new Promise<string>();
        promises.Enqueue(promise);
        handler?.RequestGetText();
        return promise;
    }

    public Clipboard(IClipboardHandler handler)
    {
        this.handler = handler;
        handler.Initialize(OnReadText);
    }

    private void OnReadText(string text, bool error)
    {
        while (promises.Count > 0)
        {
            var promise = promises.Dequeue();
            if (error)
            {
                promise.Reject(text);
            }
            else
            {
                promise.Resolve(text);
            }
        }
    }
}