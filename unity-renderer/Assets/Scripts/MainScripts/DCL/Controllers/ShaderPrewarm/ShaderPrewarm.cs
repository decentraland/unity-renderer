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

        private readonly IAddressableResourceProvider addressables;
        private bool areShadersPrewarm;

        public ShaderPrewarm(IAddressableResourceProvider addressables)
        {
            this.addressables = addressables;
        }

        public async UniTask PrewarmAsync(Action<float> progressCallback, CancellationToken cancellationToken)
        {
            if (areShadersPrewarm) return;

            await UniTask.Yield();

            var variantsData = await addressables.GetAddressable<ShaderVariantsData>(SHADER_VARIANTS_ASSET, cancellationToken);

            int length = variantsData.collections.Length;

            for (var i = 0; i < length; i++)
            {
                ShaderVariantCollection collection = variantsData.collections[i];

                progressCallback.Invoke(i / (float)length);
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationToken);

                collection.WarmUp();
            }

            areShadersPrewarm = true;
        }
    }
}
