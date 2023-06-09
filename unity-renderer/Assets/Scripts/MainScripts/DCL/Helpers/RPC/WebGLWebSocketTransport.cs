using HybridWebSocket;
using rpc_csharp.transport;
using System;

namespace RPC.Transports
{
    public class WebGLWebSocketTransport : ITransport
    {
        private readonly WebSocket webSocket;

        public event Action OnCloseEvent;
        public event Action<string> OnErrorEvent;
        public event Action<byte[]> OnMessageEvent;
        public event Action OnConnectEvent;

        public WebGLWebSocketTransport(WebSocket webSocket)
        {
            this.webSocket = webSocket;

            webSocket.OnMessage += HandleMessage;
            webSocket.OnError += HandleError;
            webSocket.OnClose += HandleClose;
            webSocket.OnOpen += HandleOpen;
        }

        public void Connect() =>
            webSocket.Connect();

        public void Close() =>
            webSocket.Close();

        private void HandleMessage(byte[] data) =>
            OnMessageEvent?.Invoke(data);

        private void HandleError(string error) =>
            OnErrorEvent?.Invoke(error);

        private void HandleClose(WebSocketCloseCode code) =>
            OnCloseEvent?.Invoke();

        private void HandleOpen() =>
            OnConnectEvent?.Invoke();

        public void SendMessage(byte[] data) =>
            webSocket.Send(data);

        public void Dispose()
        {
            OnCloseEvent = null;
            OnErrorEvent = null;
            OnMessageEvent = null;
            OnConnectEvent = null;

            webSocket.OnMessage -= HandleMessage;
            webSocket.OnError -= HandleError;
            webSocket.OnClose -= HandleClose;
            webSocket.OnOpen -= HandleOpen;
        }
    }
}
