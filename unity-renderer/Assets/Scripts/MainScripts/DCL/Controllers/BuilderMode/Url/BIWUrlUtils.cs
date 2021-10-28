using System.Collections;
using System.Collections.Generic;
using DCL.Configuration;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public static class BIWUrlUtils
{
    public static string GetUrlSceneObjectContent() { return BIWSettings.BASE_URL_SCENE_OBJECT_CONTENT.Replace("{ENV}", GetEnvBase()); }

    public static string GetBuilderAPIBaseUrl()
    {
        return BIWSettings.BASE_URL_BUILDER_API.Replace("{ENV}", GetEnvBase());
    }
    
    public static string GetUrlCatalog(string ethAddress)
    {
        string paramToAdd = "default";
        if (!string.IsNullOrEmpty(ethAddress))
            paramToAdd = ethAddress;
        return BIWSettings.BASE_URL_CATALOG.Replace("{ENV}", GetEnvBase()) + paramToAdd;
    }

    public static string GetUrlAssetPackContent() { return BIWSettings.BASE_URL_ASSETS_PACK_CONTENT.Replace("{ENV}", GetEnvBase()); }

    private static string GetEnvBase()
    {
        if (KernelConfig.i.Get().network == "mainnet")
            return "org";

        return "io";
    }
}