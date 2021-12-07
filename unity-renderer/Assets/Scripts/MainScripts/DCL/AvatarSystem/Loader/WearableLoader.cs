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
            if (ct.IsCancellationRequested)
            {
                Dispose();
                return;
            }

            bool bodyshapeDirty = currentSettings.bodyshapeId != settings.bodyshapeId;
            currentSettings = settings;
            if (status == IWearableLoader.Status.Succeeded && !bodyshapeDirty)
            {
                PrepareMaterials(currentSettings.skinColor, currentSettings.hairColor);
                return;
            }

            retriever.Dispose();

            WearableItem.Representation representation = wearable.GetRepresentation(settings.bodyshapeId);
            if (representation == null)
            {
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
                PrepareMaterials(currentSettings.skinColor, currentSettings.hairColor);
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

        private void PrepareMaterials(Color skinColor, Color hairColor)
        {
            foreach (Renderer renderer in rendereable.renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.name.ToLower().Contains("skin"))
                        material.SetColor(AvatarSystemUtils._BaseColor, skinColor);
                    else if (material.name.ToLower().Contains("hair"))
                        material.SetColor(AvatarSystemUtils._BaseColor, hairColor);
                }
            }
        }

        public void Dispose() { retriever?.Dispose(); }
    }
}