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

            UnityEngine.Debug.unityLogger.logEnabled = true;
            if (VERBOSE)
                Debug.Log($"[{nameof(GetType)}]: Requesting signed headers...");
            UnityEngine.Debug.unityLogger.logEnabled = false;

            string signResponse = await signRequest.RequestSignedHeaders(requestUrl, new Dictionary<string, string>(), ct);
            UnityEngine.Debug.unityLogger.logEnabled = true;
            if (VERBOSE)
                Debug.Log($"[{nameof(GetType)}]: Signed Headers received:\n{signResponse}");
            UnityEngine.Debug.unityLogger.logEnabled = false;

            webSocket.Connect();
            // We have to wait for connection to be done to send the signed headers for authentication
            var connected = false;
            void OnReady()
            {
                connected = true;
            }
            webSocket.OnOpen += OnReady;
            UnityEngine.Debug.unityLogger.logEnabled = true;
            if (VERBOSE)
                Debug.Log($"[{nameof(GetType)}]: Waiting for connection...");
            UnityEngine.Debug.unityLogger.logEnabled = false;
            webSocket.Connect();
            await UniTask.WaitUntil(() => connected, cancellationToken: ct);
            UnityEngine.Debug.unityLogger.logEnabled = true;
            if (VERBOSE)
                Debug.Log($"[{nameof(GetType)}]: Connected");
            webSocket.OnOpen -= OnReady;
            UnityEngine.Debug.unityLogger.logEnabled = false;

            UnityEngine.Debug.unityLogger.logEnabled = true;
            if (VERBOSE)
                Debug.Log($"[{nameof(GetType)}]: Sending the signed headers");
            UnityEngine.Debug.unityLogger.logEnabled = false;
            webSocket.Send(signResponse);
        }

        private void HandleMessage(byte[] data)
        {
            OnMessageEvent?.Invoke(data);
        }

        private void HandleError(string errorMsg)
        {
            if(VERBOSE)
                Debug.Log($"[{nameof(GetType)}]: Error\n{errorMsg}");
            OnErrorEvent?.Invoke(errorMsg);
        }

        private void HandleClose(WebSocketCloseCode closeCode)
        {
            if (VERBOSE)
                Debug.Log($"[{nameof(GetType)}]: Closed WebSocket: {closeCode}");
            OnCloseEvent?.Invoke();
        }

        private void HandleOpen()
        {
            if (VERBOSE)
                Debug.Log($"[{nameof(GetType)}]: Open WebSocket:");
            OnConnectEvent?.Invoke();
        }

        public void SendMessage(byte[] data)
        {
            if (VERBOSE)
                Debug.Log($"[{nameof(GetType)}]: Sending bytes UTF8 decoded:\n{Encoding.UTF8.GetString(data)}");
            webSocket.Send(data);
        }

        public void SendMessage(string data)
        {
            if (VERBOSE)
                Debug.Log($"[{nameof(GetType)}]: Sending data:\n{data}");
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
