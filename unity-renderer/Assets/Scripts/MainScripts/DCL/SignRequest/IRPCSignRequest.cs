using Cysharp.Threading.Tasks;
using Decentraland.Renderer.KernelServices;
using System.Collections.Generic;
using System.Threading;

public interface IRPCSignRequest
{
    UniTask<SignBodyResponse> RequestSignedRequest(RequestMethod method, string url, string metadata, CancellationToken cancellationToken);

    UniTask<string> RequestSignedHeaders(string url, IDictionary<string, string> metadata, CancellationToken cancellationToken);
}
