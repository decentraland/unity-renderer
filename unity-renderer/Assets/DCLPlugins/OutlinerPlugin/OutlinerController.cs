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

    private CancellationTokenSource animationCTS = new CancellationTokenSource();

    public OutlinerController(DataStore_Outliner dataStore, OutlineRenderersSO outlineRenderersSO)
    {
        this.dataStore = dataStore;
        this.outlineRenderersSO = outlineRenderersSO;

        outlineScreenEffectFeature = GetOutlineScreenEffectFeature();
        outlineScreenEffectFeature.settings.effectFade = 0;

        this.dataStore.avatarOutlined.OnAdded += OnAvatarOutlinedAdded;
        this.dataStore.avatarOutlined.OnRemoved += OnAvatarOutlinedRemoved;
    }

    private OutlineScreenEffectFeature GetOutlineScreenEffectFeature()
    {
        // Unity refuses to make this accessible.
        // Some alternatives could be moving ForwardRenderer to Resources
        // Have an ScriptableObject with all the references somewhere.
        // I find reflection cleaner.

        var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);
        FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
        var scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];

        return scriptableRendererData?.rendererFeatures.OfType<OutlineScreenEffectFeature>().First();
    }

    private void OnAvatarOutlinedAdded(Renderer renderer)
    {
        outlineRenderersSO.avatars.Add((renderer, renderer.GetComponent<MeshFilter>().sharedMesh.subMeshCount));
        UpdateFadeTarget(1);
    }

    private void OnAvatarOutlinedRemoved(Renderer rendererToRemove)
    {
        outlineRenderersSO.avatars.RemoveAll((avatarWithMeshCount) => avatarWithMeshCount.avatar == rendererToRemove);

        if (outlineRenderersSO.avatars.Count > 0)
            UpdateFadeTarget(1);
        else
            UpdateFadeTarget(0);
    }

    private void UpdateFadeTarget(float newTargetFade)
    {
        animationCTS?.Cancel();
        animationCTS?.Dispose();
        animationCTS = new CancellationTokenSource();
        SetOutlineFadeAsync(newTargetFade, animationCTS.Token).Forget();
    }

    private async UniTaskVoid SetOutlineFadeAsync(float target, CancellationToken ct = default)
    {
        const float SPEED_IN = 1f;
        const float SPEED_OUT = 3f;

        float speed = target > 0 ? SPEED_IN : SPEED_OUT;
        var settings = outlineScreenEffectFeature.settings;

        try
        {
            ct.ThrowIfCancellationRequested();

            while (!Mathf.Approximately(settings.effectFade, target))
            {
                settings.effectFade = Mathf.MoveTowards(outlineScreenEffectFeature.settings.effectFade, target, speed * Time.deltaTime);
                await UniTask.NextFrame(ct);
            }

            settings.effectFade = target;
        }
        catch (OperationCanceledException)
        {
            //Do nothing
        }
    }

    public void Dispose()
    {
        this.dataStore.avatarOutlined.OnAdded -= OnAvatarOutlinedAdded;
        this.dataStore.avatarOutlined.OnRemoved -= OnAvatarOutlinedRemoved;
        animationCTS?.Cancel();
        animationCTS?.Dispose();
        animationCTS = null;
    }
}
