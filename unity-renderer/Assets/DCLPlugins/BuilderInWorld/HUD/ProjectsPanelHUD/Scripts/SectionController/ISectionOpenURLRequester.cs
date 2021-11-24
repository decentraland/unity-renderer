using System;

/// <summary>
/// This interface will release an event when the inheritor want to open an URL
/// </summary>
internal interface ISectionOpenURLRequester
{
    /// <summary>
    /// This action will be launched when we want to open an URL 
    /// </summary>
    event Action<string>  OnRequestOpenUrl;
}