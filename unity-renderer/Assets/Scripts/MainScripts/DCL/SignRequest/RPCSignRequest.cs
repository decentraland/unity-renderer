using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;
using Decentraland.Renderer.KernelServices;
using Google.Protobuf.Collections;
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

    public async UniTask<string> RequestSignedHeaders(string url, IDictionary<string, string> metadata, CancellationToken cancellationToken)
    {
        var request = new GetSignedHeadersRequest() { Url = url };
        request.Metadata.Add(metadata);
        var response = await rpc.SignRequestKernelService()
                                .GetSignedHeaders(request)
                                .AttachExternalCancellation(cancellationToken);
        return response.Message;
    }
}
