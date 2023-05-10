using Cysharp.Threading.Tasks;
using System;

namespace MainScripts.DCL.Controllers.ShaderPrewarm
{
    public interface IShaderPrewarm
    {
        UniTask PrewarmAsync(Action<float> progressCallback);
    }
}
