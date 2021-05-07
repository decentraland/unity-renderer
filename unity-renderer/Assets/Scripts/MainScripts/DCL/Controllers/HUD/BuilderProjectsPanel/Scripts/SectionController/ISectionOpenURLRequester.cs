using System;

internal interface ISectionOpenURLRequester
{
    event Action<string>  OnRequestOpenUrl;
}