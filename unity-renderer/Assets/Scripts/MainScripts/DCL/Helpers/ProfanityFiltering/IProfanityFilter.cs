using Cysharp.Threading.Tasks;
using System.Threading;

namespace DCL.ProfanityFiltering
{
    public interface IProfanityFilter : IService
    {
        UniTask<string> Filter(string message, CancellationToken cancellationToken = default);
    }
}
