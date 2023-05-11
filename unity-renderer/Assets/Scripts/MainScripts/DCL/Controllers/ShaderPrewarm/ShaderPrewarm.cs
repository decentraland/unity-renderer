using Cysharp.Threading.Tasks;
using DCL.Providers;
using System;
using System.Threading;
using UnityEngine;
using Environment = DCL.Environment;

namespace MainScripts.DCL.Controllers.ShaderPrewarm
{
    public class ShaderPrewarm : IShaderPrewarm
    {
        private const string SHADER_VARIANTS_ASSET = "ShaderVariantsData";

        private bool areShadersPrewarm;

        public async UniTask PrewarmAsync(Action<float> progressCallback, CancellationToken cancellationToken)
        {
            if (areShadersPrewarm) return;

            await UniTask.Yield();

            var variantsData = await LoadAssets(cancellationToken);

            int length = variantsData.collections.Length;

            for (var i = 0; i < length; i++)
            {
                ShaderVariantCollection collection = variantsData.collections[i];

                progressCallback.Invoke(i/(float)length);
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken);

                collection.WarmUp();
            }

            areShadersPrewarm = true;
        }

        private static UniTask<ShaderVariantsData> LoadAssets(CancellationToken cancellationToken) =>
            Environment.i.serviceLocator.Get<IAddressableResourceProvider>().GetAddressable<ShaderVariantsData>(SHADER_VARIANTS_ASSET, cancellationToken);

    }
}
