using Cysharp.Threading.Tasks;

namespace MainScripts.DCL.Controllers.ShaderPrewarm
{
    public interface IShaderPrewarm
    {
        UniTask PrewarmAsync();
    }
}
