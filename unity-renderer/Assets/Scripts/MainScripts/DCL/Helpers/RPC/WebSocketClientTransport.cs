using HybridWebSocket;
using System;
using ITransport = rpc_csharp.transport.ITransport;

namespace RPC.Transports
{
    public class WebSocketClientTransport : ITransport
    {
        private readonly WebSocket webSocket;

        public event Action OnCloseEvent;
        public event Action<string> OnErrorEvent;
        public event Action<byte[]> OnMessageEvent;
        public event Action OnConnectEvent;

        public WebSocketClientTransport(string url)
        {
            webSocket = WebSocketFactory.CreateInstance(url);
            webSocket.OnMessage += this.HandleMessage;
            webSocket.OnError += this.HandleError;
            webSocket.OnClose += this.HandleClose;
            webSocket.OnOpen += this.HandleOpen;
        }

        public void Connect() =>
            webSocket.Connect();

        public void SendMessage(byte[] data)
        {
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

        private void HandleMessage(byte[] data)
        {
            OnMessageEvent?.Invoke(data);
        }

        private void HandleError(string errorMsg)
        {
            OnErrorEvent?.Invoke(errorMsg);
        }

        private void HandleClose(WebSocketCloseCode closeCode)
        {
            OnCloseEvent?.Invoke();
        }

        private void HandleOpen()
        {
            OnConnectEvent?.Invoke();
        }
    }
}
