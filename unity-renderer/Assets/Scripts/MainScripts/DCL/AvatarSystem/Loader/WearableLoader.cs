using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;

namespace AvatarSystem
{
    public class WearableLoader : IWearableLoader
    {
        // TODO: This should be a service
        internal static IWearableItemResolver defaultWearablesResolver = new WearableItemResolver();
        static WearableLoader()
        {
            // Prewarm default wearables
            defaultWearablesResolver.Resolve(WearableLiterals.DefaultWearables.GetDefaultWearables());
        }

        public WearableItem wearable { get; }
        public Rendereable rendereable => retriever?.rendereable;
        public IWearableLoader.Status status { get; private set; }

        private readonly IWearableRetriever retriever;
        private AvatarSettings currentSettings;

        public WearableLoader(IWearableRetriever retriever, WearableItem wearable)
        {
            this.wearable = wearable;
            this.retriever = retriever;
        }

        public async UniTask Load(GameObject container, AvatarSettings settings, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                bool bodyshapeDirty = currentSettings.bodyshapeId != settings.bodyshapeId;
                currentSettings = settings;
                if (status == IWearableLoader.Status.Succeeded && !bodyshapeDirty)
                {
                    AvatarSystemUtils.PrepareMaterialColors(rendereable, currentSettings.skinColor, currentSettings.hairColor);
                    return;
                }

                retriever.Dispose();

                try
                {
                    await LoadWearable(container, wearable, settings.bodyshapeId, ct);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    // Ignored so we try to fallback (if needed)
                }

                // Succeeded
                if (rendereable != null)
                {
                    AvatarSystemUtils.PrepareMaterialColors(rendereable, currentSettings.skinColor, currentSettings.hairColor);
                    status = IWearableLoader.Status.Succeeded;
                    return;
                }

                //Try getting a default if category is needed
                if (AvatarSystemUtils.IsCategoryRequired(wearable.data.category))
                    await FallbackToDefault(container, ct);

                if (rendereable != null)
                {
                    AvatarSystemUtils.PrepareMaterialColors(rendereable, currentSettings.skinColor, currentSettings.hairColor);
                    status = IWearableLoader.Status.Defaulted;
                }
                else
                    status = IWearableLoader.Status.Failed;
            }
            catch (OperationCanceledException)
            {
                Dispose();
                throw;
            }
        }

        public void SetBones(Transform rootBone, Transform[] bones) { AvatarSystemUtils.CopyBones(rootBone, bones, rendereable.renderers.OfType<SkinnedMeshRenderer>()); }

        private async UniTask FallbackToDefault(GameObject container, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                string wearableId = WearableLiterals.DefaultWearables.GetDefaultWearable(currentSettings.bodyshapeId, wearable.data.category);
                Debug.Log($"Falling back {wearable.id} to wearable {wearableId}");

                WearableItem defaultWearable = await defaultWearablesResolver.Resolve(wearableId, ct);

                await LoadWearable(container, defaultWearable, currentSettings.bodyshapeId, ct);
            }
            catch (OperationCanceledException)
            {
                //No disposing required
                throw;
            }

        }

        private async UniTask LoadWearable(GameObject container, WearableItem wearableToLoad, string bodyshapeId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            WearableItem.Representation representation = wearableToLoad.GetRepresentation(bodyshapeId);
            if (representation == null)
            {
                Debug.Log($"No representation for {bodyshapeId} of {wearableToLoad.id}");
                return;
            }

            await retriever.Retrieve(container, wearableToLoad.GetContentProvider(bodyshapeId), wearableToLoad.baseUrlBundles, representation.mainFile, ct);
        }

        public void Dispose() { retriever?.Dispose(); }
    }
}