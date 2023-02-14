using DCL;
using DCL.Models;
using System;
using System.Collections.Generic;

/*
* Scene utils file for media related stuff
*/

public static partial class UtilsScene
{
    // We are currently accepting all scene's required permissions.
    // In the future we might want to replace this for user's accepted permissions.
    public static bool HasRequiredPermission(IReadOnlyList<string> requiredPermissions, string permission)
    {
        for (int i = 0; i < requiredPermissions.Count; i++)
        {
            if (requiredPermissions[i] == permission)
                return true;
        }

        return false;
    }

    public static bool IsUrlDomainAllowed(IReadOnlyList<string> allowedDomains, string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
        {
            for (int i = 0; i < allowedDomains.Count; i++)
            {
                if (allowedDomains[i] == uri.Host)
                    return true;
            }
        }

        return false;
    }

    public static bool TrySetMediaUrl(string inputUrl, ContentProvider sceneContentProvider,
        IReadOnlyList<string> sceneRequiredPermissions, IReadOnlyList<string> sceneAllowedMediaHostnames,
        out string url)
    {
        if (string.IsNullOrEmpty(inputUrl))
        {
            url = string.Empty;
            return false;
        }

        if (sceneContentProvider.TryGetContentsUrl(inputUrl, out url))
        {
            return true;
        }

        if (sceneRequiredPermissions == null || sceneAllowedMediaHostnames == null)
        {
            url = string.Empty;
            return false;
        }

        if (HasRequiredPermission(sceneRequiredPermissions, ScenePermissionNames.ALLOW_MEDIA_HOSTNAMES)
            && IsUrlDomainAllowed(sceneAllowedMediaHostnames, inputUrl))
        {
            url = inputUrl;
            return true;
        }

        url = string.Empty;
        return false;
    }
}
