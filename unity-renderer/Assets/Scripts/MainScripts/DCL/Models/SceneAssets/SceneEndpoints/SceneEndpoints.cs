using DCL.Configuration;

public static class SceneEndpoints
{
    public static string GetUrlSceneObjectContent() 
    { 
        return AssetCatalogUrlSettings.BASE_URL_SCENE_OBJECT_CONTENT.Replace("{ENV}", GetEnvBase()); 
    }

    public static string GetManifestJSON(string jsonManifest) { return "{\"manifest\":" + jsonManifest + "}"; }

    public static string GetBuilderProjecThumbnailUrl(string projectId, string filename)
    {
        string resolvedUrl = GetResolvedEnviromentUrl(AssetCatalogUrlSettings.BASE_URL_BUILDER_PROJECT_THUMBNAIL);
        resolvedUrl = resolvedUrl.Replace("{id}", projectId) +filename;
        return resolvedUrl;
    }
    
    public static string GetUrlAssetPackContent() 
    { 
        return GetResolvedEnviromentUrl(AssetCatalogUrlSettings.BASE_URL_ASSETS_PACK_CONTENT); 
    }

    private static string GetResolvedEnviromentUrl(string url)
    {
        return url.Replace("{ENV}", GetEnvBase());
    }
    
    private static bool IsMainNet()
    {
        return KernelConfig.i.Get().network == "mainnet";
    }
    
    private static string GetEnvBase()
    {
        if (IsMainNet())
            return "org";

        return "io";
    }
}