using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace MainScripts.DCL.Controllers.ShaderPrewarm
{
    public interface IShaderPrewarm
    {
        UniTask PrewarmAsync(Action<float> progressCallback, CancellationToken cancellationToken);
    }
}
