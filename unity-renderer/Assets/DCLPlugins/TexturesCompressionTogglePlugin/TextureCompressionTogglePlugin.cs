using DCL;

public class TextureCompressionTogglePlugin : IPlugin
{
    public TextureCompressionTogglePlugin()
    {
        DataStore.i.textureConfig.runCompression.Set(true);
    }

    public void Dispose() { }
}
