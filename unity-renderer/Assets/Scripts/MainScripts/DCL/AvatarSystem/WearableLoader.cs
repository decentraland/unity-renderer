using System.Collections;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Components;
using DCL.Helpers;
using UnityEngine;

public class WearableLoader : IWearableLoader
{
    public WearableItem wearable { get; }
    public Rendereable rendereable => retriever?.rendereable;
    public IWearableLoader.Status status { get; private set; }

    private readonly IWearableRetriever retriever;
    private AvatarSettings currentSettings;

    public WearableLoader( IWearableRetriever retriever, WearableItem wearable)
    {
        this.wearable = wearable;
        this.retriever = retriever;
    }

    public async UniTask Load(GameObject container, AvatarSettings settings)
    {
        // TODO: Add cancellation token
        bool bodyshapeDirty = currentSettings.bodyshapeId != settings.bodyshapeId;
        currentSettings = settings;
        if (status == IWearableLoader.Status.Succeeded && !bodyshapeDirty)
        {
            PrepareMaterials();
            return;
        }

        retriever.Dispose();

        WearableItem.Representation representation = wearable.GetRepresentation(settings.bodyshapeId);
        if (representation == null)
        {
            status = IWearableLoader.Status.Failed;
            if (AvatarSystemUtils.IsCategoryRequired(wearable.data.category))
                await FallbackToDefault();
            return;
        }

        await retriever.Retrieve(container, wearable.GetContentProvider(settings.bodyshapeId), wearable.baseUrlBundles, representation.mainFile);


        var skmr = retriever.rendereable.container.GetComponentsInChildren<SkinnedMeshRenderer>();
        Debug.Log("----------");
        Debug.Log($"Container: {retriever.rendereable.container.transform.GetHierarchyPath()}");
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skmr)
            Debug.Log(skinnedMeshRenderer.transform.GetHierarchyPath());
        foreach (var x in rendereable.renderers)
            Debug.Log(x.transform.GetHierarchyPath());

        if (rendereable != null)
        {
            status = IWearableLoader.Status.Succeeded;
            return;
        }

        status = IWearableLoader.Status.Failed;

        if (AvatarSystemUtils.IsCategoryRequired(wearable.data.category))
            await FallbackToDefault();
    }

    private async UniTask FallbackToDefault()
    {
        status = IWearableLoader.Status.Failed;
        //TODO load a default wearable
    }

    private void PrepareMaterials() { }

    public void Dispose() { retriever?.Dispose(); }
}

public class WearableRetriever : IWearableRetriever
{
    public Rendereable rendereable { get; private set; }

    private RendereableAssetLoadHelper loaderAssetHelper;

    public async UniTask<Rendereable> Retrieve(GameObject container, ContentProvider contentProvider, string baseUrl, string mainFile)
    {
        // TODO: Add cancellation token
        loaderAssetHelper?.Unload();

        loaderAssetHelper = new RendereableAssetLoadHelper(contentProvider, baseUrl);

        loaderAssetHelper.settings.forceNewInstance = false;
        loaderAssetHelper.settings.initialLocalPosition = Vector3.up * 0.75f;
        loaderAssetHelper.settings.cachingFlags = MaterialCachingHelper.Mode.CACHE_SHADERS;
        //TODO set as invisible
        loaderAssetHelper.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITHOUT_TRANSITION;
        loaderAssetHelper.settings.parent = container.transform;
        loaderAssetHelper.settings.layer = container.layer;

        bool done = false;

        void OnSuccessWrapper(Rendereable rendereable)
        {
            Debug.Log($"{baseUrl}{mainFile}");
            loaderAssetHelper?.ClearEvents();
            done = true;
            this.rendereable = rendereable;
        }

        void OnFailEventWrapper()
        {
            loaderAssetHelper?.ClearEvents();
            done = true;
            rendereable = null;
        }

        loaderAssetHelper.OnSuccessEvent += OnSuccessWrapper;
        loaderAssetHelper.OnFailEvent += OnFailEventWrapper;
        loaderAssetHelper.Load(mainFile, AvatarSystemUtils.UseAssetBundles() ? RendereableAssetLoadHelper.LoadingType.ASSET_BUNDLE_WITH_GLTF_FALLBACK : RendereableAssetLoadHelper.LoadingType.GLTF_ONLY);
        await UniTask.WaitUntil(() => done);
        return (rendereable);
    }

    public void Dispose() { loaderAssetHelper?.Unload(); }
}