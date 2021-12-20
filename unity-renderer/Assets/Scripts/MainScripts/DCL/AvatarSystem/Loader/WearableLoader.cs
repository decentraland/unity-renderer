using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;

namespace AvatarSystem
{
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

        public async UniTask Load( GameObject container, AvatarSettings settings, CancellationToken ct = default)
        {
            bool bodyshapeDirty = currentSettings.bodyshapeId != settings.bodyshapeId;
            currentSettings = settings;
            if (status == IWearableLoader.Status.Succeeded && !bodyshapeDirty)
            {
                AvatarSystemUtils.PrepareMaterialColors(rendereable, currentSettings.skinColor, currentSettings.hairColor);
                return;
            }

            if (ct.IsCancellationRequested)
            {
                Dispose();
                return;
            }

            retriever.Dispose();

            WearableItem.Representation representation = wearable.GetRepresentation(settings.bodyshapeId);
            if (representation == null)
            {
                Debug.Log($"No representation for {settings.bodyshapeId} of {wearable.id}");
                status = IWearableLoader.Status.Failed;
                if (AvatarSystemUtils.IsCategoryRequired(wearable.data.category))
                    await FallbackToDefault(ct);

                if (ct.IsCancellationRequested)
                {
                    Dispose();
                }
                return;
            }

            await retriever.Retrieve(container, wearable.GetContentProvider(settings.bodyshapeId), wearable.baseUrlBundles, representation.mainFile, ct);
            if (ct.IsCancellationRequested)
            {
                Dispose();
                return;
            }

            if (rendereable != null)
            {
                status = IWearableLoader.Status.Succeeded;
                AvatarSystemUtils.PrepareMaterialColors(rendereable, currentSettings.skinColor, currentSettings.hairColor);
                return;
            }

            status = IWearableLoader.Status.Failed;

            if (AvatarSystemUtils.IsCategoryRequired(wearable.data.category))
            {
                await FallbackToDefault(ct);
                if (ct.IsCancellationRequested)
                {
                    Dispose();
                }
            }
        }

        private async UniTask FallbackToDefault(CancellationToken ct)
        {
            status = IWearableLoader.Status.Failed;
            //TODO load a default wearable
        }

        public void Dispose() { retriever?.Dispose(); }
    }
}