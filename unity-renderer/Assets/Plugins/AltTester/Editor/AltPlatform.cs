namespace Altom.AltTesterEditor
{
    public enum AltPlatform
    {
        Android,
#if UNITY_EDITOR_OSX
        iOS,
#endif
        Editor,
        Standalone,
        WebGL
    }
}
