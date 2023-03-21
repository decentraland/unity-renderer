using DCL;
using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

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

    public static bool TryGetMediaUrl(string inputUrl, ContentProvider sceneContentProvider,
        IReadOnlyList<string> sceneRequiredPermissions, IReadOnlyList<string> sceneAllowedMediaHostnames,
        out string url)
    {
        if (string.IsNullOrEmpty(inputUrl))
        {
            url = string.Empty;
            Debug.LogError("Video URL is empty.");
            return false;
        }

        if (sceneContentProvider.TryGetContentsUrl(inputUrl, out url))
        {
            return true;
        }

        if (sceneRequiredPermissions == null || sceneAllowedMediaHostnames == null)
        {
            url = string.Empty;
            Debug.LogError("Video URL nullified due to not having 'allowedMediaHostnames' scene.json file.");
            return false;
        }

        if (HasRequiredPermission(sceneRequiredPermissions, ScenePermissionNames.ALLOW_MEDIA_HOSTNAMES)
            && IsUrlDomainAllowed(sceneAllowedMediaHostnames, inputUrl))
        {
            url = inputUrl;
            return true;
        }

        Debug.LogError("Video URL nullified due to its host domain not being in scene's 'allowedMediaHostnames'. Please add the video's host domain in 'allowedMediaHostnames' in the scene.json file.");

        url = string.Empty;
        return false;
    }
}
