using Cysharp.Threading.Tasks;

namespace DCL.ProfanityFiltering
{
    public interface IProfanityFilter : IService
    {
        UniTask<string> Filter(string message);
    }
}
