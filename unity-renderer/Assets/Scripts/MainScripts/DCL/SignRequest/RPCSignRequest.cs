using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;
using Decentraland.Renderer.KernelServices;
using System;
using System.Collections.Generic;
using SignBodyRequest = Decentraland.Renderer.KernelServices.SignBodyRequest;
using SignBodyResponse = Decentraland.Renderer.KernelServices.SignBodyResponse;

public class RPCSignRequest : IRPCSignRequest
{
    private const int REQUEST_TIMEOUT = 30;

    private readonly IRPC rpc;

    public RPCSignRequest(IRPC rpc)
    {
        this.rpc = rpc;
    }

    public async UniTask<SignBodyResponse> RequestSignedRequest(RequestMethod method, string url, string metadata, CancellationToken cancellationToken)
    {
        SignBodyResponse response = await rpc.SignRequestKernelService()
                                             .GetRequestSignature(new SignBodyRequest()
                                              {
                                                  Method = method,
                                                  Url = url,
                                                  Metadata = string.IsNullOrEmpty(metadata) ? "{}" : metadata
                                              })
                                             .AttachExternalCancellation(cancellationToken)
                                             .Timeout(TimeSpan.FromSeconds(REQUEST_TIMEOUT));
        return response;
    }

}
