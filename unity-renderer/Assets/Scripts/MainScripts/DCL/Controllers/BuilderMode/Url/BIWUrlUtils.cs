using System.Collections;
using System.Collections.Generic;
using DCL.Configuration;
using UnityEngine;

public static class BIWUrlUtils
{
    public static string GetUrlSceneObjectContent() { return BuilderInWorldSettings.BASE_URL_SCENE_OBJECT_CONTENT.Replace("{ENV}", GetEnvBase()); }

    public static string GetUrlCatalog() { return BuilderInWorldSettings.BASE_URL_CATALOG.Replace("{ENV}", GetEnvBase()); }

    public static string GetUrlAssetPackContent() { return BuilderInWorldSettings.BASE_URL_ASSETS_PACK_CONTENT.Replace("{ENV}", GetEnvBase()); }

    private static string GetEnvBase()
    {
        if (KernelConfig.i.Get().tld == "org")
            return "org";

        return "io";
    }
}