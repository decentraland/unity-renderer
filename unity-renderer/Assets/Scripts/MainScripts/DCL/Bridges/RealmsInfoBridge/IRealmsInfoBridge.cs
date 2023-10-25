using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Variables.RealmsInfo;

namespace DCL
{
    public interface IRealmsInfoBridge
    {
        UniTask<IReadOnlyList<RealmModel>> FetchRealmsInfo(CancellationToken cancellationToken);
    }
}
