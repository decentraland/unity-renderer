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