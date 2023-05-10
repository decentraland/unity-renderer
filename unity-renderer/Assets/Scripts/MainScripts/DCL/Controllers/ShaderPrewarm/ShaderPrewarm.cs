using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using UnityEngine;

namespace MainScripts.DCL.Controllers.ShaderPrewarm
{
    public class ShaderPrewarm : IShaderPrewarm
    {
        private const string SHADER_VARIANTS_ASSET = "shadervariants-selected";

        private bool areShadersPrewarm;

        public async UniTask PrewarmAsync()
        {
            if (areShadersPrewarm) return;

            await UniTask.Yield();

            var shaderVariants = await GetShaderVariantsAsset();

            shaderVariants.WarmUp();
            areShadersPrewarm = true;
        }

        private static UniTask<ShaderVariantCollection> GetShaderVariantsAsset() =>
            Environment.i.serviceLocator.Get<IAddressableResourceProvider>().GetAddressable<ShaderVariantCollection>(SHADER_VARIANTS_ASSET);

    }
}
