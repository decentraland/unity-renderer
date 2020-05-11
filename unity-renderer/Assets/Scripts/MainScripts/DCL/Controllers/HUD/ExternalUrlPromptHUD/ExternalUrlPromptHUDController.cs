using DCL;
using DCL.Controllers;
using UnityEngine;
using System;
using System.Collections.Generic;
using DCL.Interface;

public class ExternalUrlPromptHUDController : IHUD
{
    internal ExternalUrlPromptView view { get; private set; }

    internal Dictionary<string, HashSet<string>> trustedDomains = new Dictionary<string, HashSet<string>>();

    public ExternalUrlPromptHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("ExternalUrlPromptHUD")).GetComponent<ExternalUrlPromptView>();
        view.name = "_ExternalUrlPromptHUD";
        view.content.SetActive(false);

        if (SceneController.i)
            SceneController.i.OnOpenExternalUrlRequest += ProcessOpenUrlRequest;
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);
    }

    public void Dispose()
    {
        if (SceneController.i)
            SceneController.i.OnOpenExternalUrlRequest -= ProcessOpenUrlRequest;

        trustedDomains.Clear();
    }

    internal void ProcessOpenUrlRequest(ParcelScene scene, string url)
    {
        Uri uri;
        if (Uri.TryCreate(url, UriKind.Absolute, out uri))
        {
            if (trustedDomains.ContainsKey(scene.sceneData.id) && trustedDomains[scene.sceneData.id].Contains(uri.Host))
            {
                OpenUrl(url);
                return;
            }

            view.RequestOpenUrl(uri, result =>
            {
                switch (result)
                {
                    case ExternalUrlPromptView.ResultType.APPROVED_TRUSTED:
                        if (!trustedDomains.ContainsKey(scene.sceneData.id))
                        {
                            trustedDomains.Add(scene.sceneData.id, new HashSet<string>());
                        }
                        trustedDomains[scene.sceneData.id].Add(uri.Host);
                        OpenUrl(url);
                        break;
                    case ExternalUrlPromptView.ResultType.APPROVED:
                        OpenUrl(url);
                        break;
                }
            });
        }
    }

    private void OpenUrl(string url)
    {
        WebInterface.OpenURL(url);
    }
}
