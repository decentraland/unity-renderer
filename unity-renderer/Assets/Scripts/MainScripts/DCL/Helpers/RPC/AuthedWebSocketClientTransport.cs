using Cysharp.Threading.Tasks;
using HybridWebSocket;
using rpc_csharp.transport;
using Sentry.Unity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace RPC.Transports
{
    /// <summary>
    /// This WebSocketClientTransport contains authentication with signed headers out of the box. The process is the following:
    /// 1. Request the signed headers using the IRPCSignRequest provided.
    /// 2. Connect to the WebSocket server.
    /// 3. Send the signed headers to the server.
    /// </summary>
    public class AuthedWebSocketClientTransport : ITransport
    {
        private bool VERBOSE = true;

        public event Action OnCloseEvent;
        public event Action<string> OnErrorEvent;
        public event Action<byte[]> OnMessageEvent;
        public event Action OnConnectEvent;

        private readonly IRPCSignRequest signRequest;
        private readonly WebSocket webSocket;

        public AuthedWebSocketClientTransport(IRPCSignRequest signRequest, string url)
        {
            webSocket = WebSocketFactory.CreateInstance(url);
            this.signRequest = signRequest;
        }

        public async UniTask Connect(string requestUrl, CancellationToken ct = default)
        {
            webSocket.OnMessage += this.HandleMessage;
            webSocket.OnError += this.HandleError;
            webSocket.OnClose += this.HandleClose;
            webSocket.OnOpen += this.HandleOpen;

            if (VERBOSE)
                Debug.Log($"[{GetType().Name}]: Requesting signed headers...");

            string signResponse = await signRequest.RequestSignedHeaders(requestUrl, new Dictionary<string, string>(), ct);
            if (VERBOSE)
                Debug.Log($"[{GetType().Name}]: Signed Headers received:\n{signResponse}");

            var connected = false;
            void OnReady()
            {
                if (VERBOSE)
                    Debug.Log($"[{GetType().Name}]: Open called");
                webSocket.OnOpen -= OnReady;
                connected = true;
            }
            webSocket.OnOpen += OnReady;
            webSocket.Connect();
            // We have to wait for connection to be done to send the signed headers for authentication
            if (VERBOSE)
                Debug.Log($"[{GetType().Name}]: Waiting for connection...");
            await UniTask.WaitUntil(() => connected, cancellationToken: ct);
            if (VERBOSE)
                Debug.Log($"[{GetType().Name}]: Connected");
            webSocket.OnOpen -= OnReady;

            if (VERBOSE)
                Debug.Log($"[{GetType().Name}]: Sending the signed headers");
            webSocket.Send(signResponse);
        }

        private void HandleMessage(byte[] data)
        {
            OnMessageEvent?.Invoke(data);
        }

        private void HandleError(string errorMsg)
        {
            if(VERBOSE)
                Debug.Log($"[{GetType().Name}]: Error\n{errorMsg}");
            OnErrorEvent?.Invoke(errorMsg);
        }

        private void HandleClose(WebSocketCloseCode closeCode)
        {
            if (VERBOSE)
                Debug.Log($"[{GetType().Name}]: Closed WebSocket: {closeCode}");
            OnCloseEvent?.Invoke();
        }

        private void HandleOpen()
        {
            if (VERBOSE)
                Debug.Log($"[{GetType().Name}]: Open WebSocket:");
            OnConnectEvent?.Invoke();
        }

        public void SendMessage(byte[] data)
        {
            if (VERBOSE)
                Debug.Log($"[{GetType().Name}]: Sending bytes UTF8 decoded:\n{Encoding.UTF8.GetString(data)}");
            webSocket.Send(data);
        }

        public void SendMessage(string data)
        {
            if (VERBOSE)
                Debug.Log($"[{GetType().Name}]: Sending data:\n{data}");
            webSocket.Send(data);
        }

        public void Close()
        {
            webSocket.Close();
        }

        public void Dispose()
        {
            OnCloseEvent = null;
            OnErrorEvent = null;
            OnMessageEvent = null;
            OnConnectEvent = null;

            webSocket.OnMessage -= this.HandleMessage;
            webSocket.OnError -= this.HandleError;
            webSocket.OnClose -= this.HandleClose;
            webSocket.OnOpen -= this.HandleOpen;
        }
    }
}
