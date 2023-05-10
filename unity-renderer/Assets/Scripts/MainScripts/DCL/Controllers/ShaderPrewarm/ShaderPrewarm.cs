using Cysharp.Threading.Tasks;
using DCL.Providers;
using System;
using System.Collections.Generic;
using UnityEngine;
using Environment = DCL.Environment;

namespace MainScripts.DCL.Controllers.ShaderPrewarm
{
    public class ShaderPrewarm : IShaderPrewarm
    {
        private const string SHADER_VARIANTS_ASSET = "ShaderVariantsGroup";

        private bool areShadersPrewarm;

        public async UniTask PrewarmAsync(Action<float> progressCallback)
        {
            if (areShadersPrewarm) return;

            await UniTask.Yield();

            var assets = await LoadAssets();

            for (var i = 0; i < assets.Count; i++)
            {
                ShaderVariantCollection collection = assets[i];

                progressCallback.Invoke(i/(float)assets.Count);
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

                collection.WarmUp();
            }

            areShadersPrewarm = true;
        }

        private static UniTask<IList<ShaderVariantCollection>> LoadAssets() =>
            Environment.i.serviceLocator.Get<IAddressableResourceProvider>().GetAddressablesList<ShaderVariantCollection>(SHADER_VARIANTS_ASSET);

    }
}
