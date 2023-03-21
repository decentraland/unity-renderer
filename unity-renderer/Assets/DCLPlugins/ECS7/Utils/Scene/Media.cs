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
            Debug.LogError("Video playback aborted: video URL is empty.");
            return false;
        }

        if (sceneContentProvider.TryGetContentsUrl(inputUrl, out url))
        {
            return true;
        }

        if (sceneRequiredPermissions == null || sceneAllowedMediaHostnames == null)
        {
            url = string.Empty;
            Debug.LogError("Video playback aborted: 'allowedMediaHostnames' missing in scene.json file.");
            return false;
        }

        if (HasRequiredPermission(sceneRequiredPermissions, ScenePermissionNames.ALLOW_MEDIA_HOSTNAMES)
            && IsUrlDomainAllowed(sceneAllowedMediaHostnames, inputUrl))
        {
            url = inputUrl;
            return true;
        }

        Debug.LogError($"Video playback aborted: '{inputUrl}' host name is not in 'allowedMediaHostnames' in scene.json file.");

        url = string.Empty;
        return false;
    }
}
