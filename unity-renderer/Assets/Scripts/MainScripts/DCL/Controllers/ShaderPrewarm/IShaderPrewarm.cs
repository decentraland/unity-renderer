using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace MainScripts.DCL.Controllers.ShaderPrewarm
{
    public interface IShaderPrewarm
    {
        UniTask PrewarmAsync(Action<ShaderVariantCollection, float> progressCallback, CancellationToken cancellationToken);
    }
}
