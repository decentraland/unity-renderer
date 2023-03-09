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

    private static RPCSignRequest i;

    private readonly IRPC rpc;

    public static RPCSignRequest CreateSharedInstance(IRPC rpc)
    {
        i = new RPCSignRequest(rpc);
        return i;
    }

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
                                             .Timeout(TimeSpan.FromSeconds(REQUEST_TIMEOUT));
        return response;
    }

}
