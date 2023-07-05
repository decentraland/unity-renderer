using Cysharp.Threading.Tasks;
using DCL.Emotes;
using DCL.Helpers;
using DCLServices.EmotesCatalog.EmotesCatalogService;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EmotesCatalogServiceProxy : IEmotesCatalogService
{

    private const string FORCE_TO_REQUEST_WEARABLES_THROUGH_KERNEL_FF = "force_to_request_wearables_through_kernel";

    private readonly LambdasEmotesCatalogService lambdasEmotesCatalogService;
    private readonly WebInterfaceEmotesCatalogService webInterfaceEmotesCatalogService;
    private readonly KernelConfig kernelConfig;
    private readonly BaseVariable<FeatureFlag> featureFlags;
    private IEmotesCatalogService emotesCatalogServiceInUse;
    private bool isInitialized;

    public EmotesCatalogServiceProxy(LambdasEmotesCatalogService lambdasEmotesCatalogService, WebInterfaceEmotesCatalogService webInterfaceEmotesCatalogService,
        BaseVariable<FeatureFlag> featureFlags, KernelConfig kernelCofig)
    {
        this.lambdasEmotesCatalogService = lambdasEmotesCatalogService;
        this.webInterfaceEmotesCatalogService = webInterfaceEmotesCatalogService;
        this.featureFlags = featureFlags;
        this.kernelConfig = kernelCofig;
    }

    public void Initialize()
    {
        if (!featureFlags.Get().IsInitialized)
            featureFlags.OnChange += CheckFeatureFlag;
        else
            CheckFeatureFlag(featureFlags.Get());
    }

    private void CheckFeatureFlag(FeatureFlag currentFeatureFlags, FeatureFlag _ = null)
    {
        async UniTaskVoid SetServiceInUseDependingOnKernelConfig()
        {
            var currentKernelConfig = kernelConfig.EnsureConfigInitialized();
            await currentKernelConfig;
            SetCurrentService(currentKernelConfig.value.urlParamsForWearablesDebug);
        }

        featureFlags.OnChange -= CheckFeatureFlag;

        if (currentFeatureFlags.IsFeatureEnabled(FORCE_TO_REQUEST_WEARABLES_THROUGH_KERNEL_FF))
            SetCurrentService(true);
        else
            SetServiceInUseDependingOnKernelConfig().Forget();
    }

    private void SetCurrentService(bool useKernel)
    {
        if (useKernel)
            emotesCatalogServiceInUse = webInterfaceEmotesCatalogService;
        else
            emotesCatalogServiceInUse = lambdasEmotesCatalogService;
        emotesCatalogServiceInUse.Initialize();
        isInitialized = true;
    }


    public void Dispose()
    {
        emotesCatalogServiceInUse?.Dispose();
    }

    public bool TryGetLoadedEmote(string id, out WearableItem emote)
    {
        if (!isInitialized)
        {
            emote = null;
            return false;
        }
        return emotesCatalogServiceInUse.TryGetLoadedEmote(id, out emote);
    }

    public Promise<IReadOnlyList<WearableItem>> RequestOwnedEmotes(string userId) =>
        emotesCatalogServiceInUse.RequestOwnedEmotes(userId);

    public UniTask<IReadOnlyList<WearableItem>> RequestOwnedEmotesAsync(string userId, CancellationToken ct = default) =>
        emotesCatalogServiceInUse.RequestOwnedEmotesAsync(userId, ct);

    public Promise<WearableItem> RequestEmote(string id) =>
        emotesCatalogServiceInUse.RequestEmote(id);

    public List<Promise<WearableItem>> RequestEmotes(IList<string> ids) =>
        emotesCatalogServiceInUse.RequestEmotes(ids);

    public UniTask<WearableItem> RequestEmoteAsync(string id, CancellationToken ct = default) =>
        emotesCatalogServiceInUse.RequestEmoteAsync(id, ct);

    public UniTask<IReadOnlyList<WearableItem>> RequestEmotesAsync(IList<string> ids, CancellationToken ct = default) =>
        emotesCatalogServiceInUse.RequestEmotesAsync(ids, ct);

    public async UniTask<EmbeddedEmotesSO> GetEmbeddedEmotes()
    {
        if(!isInitialized)
            await UniTask.WaitUntil(() => isInitialized);
        return await emotesCatalogServiceInUse.GetEmbeddedEmotes();
    }


    public void ForgetEmote(string id) =>
        emotesCatalogServiceInUse.ForgetEmote(id);

    public void ForgetEmotes(IList<string> ids) =>
        emotesCatalogServiceInUse.ForgetEmotes(ids);
}
