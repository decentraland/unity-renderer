using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlinerController : IDisposable
{

    private readonly DataStore_Outliner dataStore;
    private readonly OutlineRenderersSO outlineRenderersSO;
    private readonly OutlineScreenEffectFeature outlineScreenEffectFeature;
    private readonly OutlineMaskFeature outlineMaskFeature;

    private CancellationTokenSource animationCTS = new CancellationTokenSource();
    private bool shouldShow;

    public OutlinerController(DataStore_Outliner dataStore, OutlineRenderersSO outlineRenderersSO)
    {
        this.dataStore = dataStore;
        this.outlineRenderersSO = outlineRenderersSO;

        outlineMaskFeature = Resources.FindObjectsOfTypeAll<OutlineMaskFeature>().FirstOrDefault();
        outlineScreenEffectFeature = Resources.FindObjectsOfTypeAll<OutlineScreenEffectFeature>().FirstOrDefault();
        outlineMaskFeature.SetActive(true);
        outlineScreenEffectFeature.SetActive(true);
        outlineScreenEffectFeature.settings.effectFade = 0;
        shouldShow = false;

        this.dataStore.avatarOutlined.OnChange += OnAvatarOutlinedChange;
    }

    private void OnAvatarOutlinedChange((Renderer renderer, int meshCount, float avatarHeight) current, (Renderer renderer, int meshCount, float avatarHeight) previous)
    {
        if (current.renderer != null)
        {
            outlineRenderersSO.avatar = current;
            //We are currently showing (or animating towards showing), nothing else to do
            if (shouldShow)
                return;

            shouldShow = true;
            animationCTS?.Cancel();
            animationCTS?.Dispose();
            animationCTS = new CancellationTokenSource();
            FadeInOutlineAsync(animationCTS.Token).Forget();
        }
        else //We have to fade out and then clear the scriptable object
        {
            //We are already fade out (or animating towards it)
            if (!shouldShow)
                return;

            shouldShow = false;
            animationCTS?.Cancel();
            animationCTS?.Dispose();
            animationCTS = new CancellationTokenSource();
            FadeOutOutlineAsync(animationCTS.Token).Forget();
        }
    }

    private async UniTaskVoid FadeInOutlineAsync(CancellationToken ct = default)
    {
        const float SPEED = 1 / 0.333f; // 300ms
        const float SHOW_FADE = 1;

        var settings = outlineScreenEffectFeature.settings;

        while (!Mathf.Approximately(settings.effectFade, SHOW_FADE))
        {
            if (ct.IsCancellationRequested)
                return;

            settings.effectFade = Mathf.MoveTowards(outlineScreenEffectFeature.settings.effectFade, SHOW_FADE, SPEED * Time.deltaTime);
            await UniTask.NextFrame(ct);
        }

        settings.effectFade = SHOW_FADE;
    }

    private async UniTaskVoid FadeOutOutlineAsync(CancellationToken ct = default)
    {
        const float SPEED = 1 / 0.333f; // 300ms
        const float HIDE_FADE = 0;

        var settings = outlineScreenEffectFeature.settings;

        while (!Mathf.Approximately(settings.effectFade, HIDE_FADE))
        {
            if (ct.IsCancellationRequested)
                return;

            settings.effectFade = Mathf.MoveTowards(outlineScreenEffectFeature.settings.effectFade, HIDE_FADE, SPEED * Time.deltaTime);
            await UniTask.NextFrame(ct);
        }

        settings.effectFade = HIDE_FADE;
        outlineRenderersSO.avatar = (null, -1, -1);
    }

    public void Dispose()
    {
        this.dataStore.avatarOutlined.OnChange -= OnAvatarOutlinedChange;
        animationCTS?.Cancel();
        animationCTS?.Dispose();
        animationCTS = null;
        outlineMaskFeature.SetActive(false);
        outlineScreenEffectFeature.SetActive(false);
    }
}
