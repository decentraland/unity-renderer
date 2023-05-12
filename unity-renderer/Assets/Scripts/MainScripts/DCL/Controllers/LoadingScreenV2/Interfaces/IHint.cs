using System;

namespace DCL.Controllers.LoadingScreenV2
{
    public interface IHint:IDisposable
    {
        string TextureUrl { get; }
        string Title { get; }
        string Body { get; }
        SourceTag SourceTag { get; }
    }


    [System.Serializable]
    public enum SourceTag
    {
        Scene = 0,
        Dcl = 1,
        Event = 2,
    }
}
