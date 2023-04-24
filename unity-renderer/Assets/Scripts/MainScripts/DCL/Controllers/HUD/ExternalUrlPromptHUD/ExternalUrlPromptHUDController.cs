using DCL;
using DCL.Controllers;
using UnityEngine;
using System;
using System.Collections.Generic;
using DCL.Interface;
using DCL.Helpers;
using RPC.Context;
using Environment = DCL.Environment;

public class ExternalUrlPromptHUDController : IHUD
{
    internal ExternalUrlPromptView view { get; private set; }

    internal Dictionary<int, HashSet<string>> trustedDomains = new Dictionary<int, HashSet<string>>();

    private readonly RestrictedActionsContext restrictedActionsServiceContext;

    public ExternalUrlPromptHUDController(RestrictedActionsContext restrictedActionsContext)
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("ExternalUrlPromptHUD")).GetComponent<ExternalUrlPromptView>();
        view.name = "_ExternalUrlPromptHUD";
        view.content.SetActive(false);

        if (Environment.i != null)
            Environment.i.world.sceneController.OnOpenExternalUrlRequest += ProcessOpenUrlRequest;

        restrictedActionsServiceContext = restrictedActionsContext;
        restrictedActionsServiceContext.OpenExternalUrlPrompt += ProcessOpenUrlRequest;
    }

    public void SetVisibility(bool visible)
    {
        view.gameObject.SetActive(visible);

        if (visible)
        {
            view.content.SetActive(true);
            view.showHideAnimator.Show();

            AudioScriptableObjects.dialogOpen.Play(true);
        }
        else
        {
            view.showHideAnimator.Hide();

            AudioScriptableObjects.dialogClose.Play(true);
        }
    }

    public void Dispose()
    {
        if (Environment.i != null)
            Environment.i.world.sceneController.OnOpenExternalUrlRequest -= ProcessOpenUrlRequest;

        trustedDomains.Clear();

        if (view != null)
            UnityEngine.Object.Destroy(view.gameObject);

        restrictedActionsServiceContext.OpenExternalUrlPrompt -= ProcessOpenUrlRequest;
    }

    internal void ProcessOpenUrlRequest(IParcelScene scene, string url)
    {
        ProcessOpenUrlRequest(url, scene.sceneData.sceneNumber);
    }

    internal bool ProcessOpenUrlRequest(string url, int sceneNumber)
    {
        Uri uri;
        if (Uri.TryCreate(url, UriKind.Absolute, out uri))
        {
            if (trustedDomains.ContainsKey(sceneNumber) && trustedDomains[sceneNumber].Contains(uri.Host))
            {
                OpenUrl(url);
                return true;
            }

            SetVisibility(true);
            Utils.UnlockCursor();
            view.RequestOpenUrl(uri, result =>
            {
                switch (result)
                {
                    case ExternalUrlPromptView.ResultType.APPROVED_TRUSTED:
                        if (!trustedDomains.ContainsKey(sceneNumber))
                        {
                            trustedDomains.Add(sceneNumber, new HashSet<string>());
                        }

                        trustedDomains[sceneNumber].Add(uri.Host);
                        OpenUrl(url);
                        break;
                    case ExternalUrlPromptView.ResultType.APPROVED:
                        OpenUrl(url);
                        break;
                }

                SetVisibility(false);
            });

            return true;
        }

        return false;
    }

    private void OpenUrl(string url)
    {
        WebInterface.OpenURL(url);
        AnalyticsHelper.SendExternalLinkAnalytic(url);
    }
}
