using Cysharp.Threading.Tasks;
using Decentraland.Renderer.KernelServices;
using System.Threading;

public interface IRPCSignRequest
{
    UniTask<SignBodyResponse> RequestSignedRequest(RequestMethod method, string url, string metadata, CancellationToken cancellationToken);
}
