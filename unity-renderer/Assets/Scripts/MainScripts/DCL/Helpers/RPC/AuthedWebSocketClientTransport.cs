using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using rpc_csharp.transport;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using WebSocketSharp;

namespace RPC.Transports
{
    public class AuthedWebSocketClientTransport : WebSocket, ITransport
    {
        private bool VERBOSE = true;

        public event Action OnCloseEvent;
        public event Action<string> OnErrorEvent;
        public event Action<byte[]> OnMessageEvent;
        public event Action OnConnectEvent;

        private readonly IRPCSignRequest signRequest;

        public AuthedWebSocketClientTransport(IRPCSignRequest signRequest, string url, params string[] protocols) : base(url, protocols)
        {
            this.signRequest = signRequest;
            this.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        }

        public new async UniTask Connect() =>
            await this.Connect(default(CancellationToken));

        public async UniTask Connect(CancellationToken ct)
        {
            base.OnMessage += this.HandleMessage;
            base.OnError += this.HandleError;
            base.OnClose += this.HandleClose;
            base.OnOpen += this.HandleOpen;

            base.Connect();

            // TODO: Quest Server is not accepting the correct url and by now it needs "/". Change it as soon as QuestServer is ready to have a generic authed WebSocket Client
            string signResponse = await signRequest.RequestSignedHeaders("/", new Dictionary<string, string>(), ct);

            if (VERBOSE)
                Debug.Log($"Signed  Headers received:\n{signResponse}");

            Send(signResponse);
        }

        private void HandleMessage(object sender, MessageEventArgs e)
        {
            OnMessageEvent?.Invoke(e.RawData);
        }

        private void HandleError(object sender, ErrorEventArgs e)
        {
            if(VERBOSE)
                Debug.Log(e.Message);
            OnErrorEvent?.Invoke(e.Message);
        }

        private void HandleClose(object sender, CloseEventArgs e)
        {
            if(VERBOSE)
                Debug.Log(e.Reason);
            OnCloseEvent?.Invoke();
        }

        private void HandleOpen(object sender, EventArgs e)
        {
            OnConnectEvent?.Invoke();
        }

        public void SendMessage(byte[] data)
        {
            Send(data);
        }

        public void Dispose()
        {
            OnCloseEvent = null;
            OnErrorEvent = null;
            OnMessageEvent = null;
            OnConnectEvent = null;

            base.OnMessage -= this.HandleMessage;
            base.OnError -= this.HandleError;
            base.OnClose -= this.HandleClose;
            base.OnOpen -= this.HandleOpen;
        }
    }
}
