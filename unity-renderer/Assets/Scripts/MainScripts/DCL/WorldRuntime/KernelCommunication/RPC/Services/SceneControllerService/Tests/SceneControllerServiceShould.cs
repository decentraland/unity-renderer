// using Cysharp.Threading.Tasks;
// using DCL;
// using DCL.Controllers;
// using DCL.CRDT;
// using DCL.Models;
// using Google.Protobuf;
// using KernelCommunication;
// using NSubstitute;
// using NUnit.Framework;
// using RPC;
// using rpc_csharp;
// using rpc_csharp.transport;
// using RPC.Services;
// using System;
// using System.Collections;
// using System.IO;
// using System.Threading;
// using UnityEngine;
// using UnityEngine.TestTools;
// using BinaryWriter = KernelCommunication.BinaryWriter;

namespace Tests
{
    public class SceneControllerServiceShould
    {
        // private RPCContext context;
        // private ITransport testClientTransport;
        // private RpcServer<RPCContext> rpcServer;
        // private CancellationTokenSource testCancellationSource;

        // [SetUp]
        public void SetUp()
        {
            // context = new RPCContext();
            //
            // var (clientTransport, serverTransport) = MemoryTransport.Create();
            // rpcServer = new RpcServer<RPCContext>();
            // rpcServer.AttachTransport(serverTransport, context);
            //
            // rpcServer.SetHandler((port, t, c) =>
            // {
            //     CRDTServiceCodeGen.RegisterService(port, new CRDTServiceImpl());
            // });
            //
            // testClientTransport = clientTransport;
            // testCancellationSource = new CancellationTokenSource();
        }

        // [TearDown]
        public void TearDown()
        {
            // rpcServer.Dispose();
            // testCancellationSource.Cancel();
            // testCancellationSource.Dispose();
        }
    }
}
