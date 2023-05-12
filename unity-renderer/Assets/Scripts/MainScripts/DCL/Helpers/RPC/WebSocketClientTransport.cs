using System;
using WebSocketSharp;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;
using ITransport = rpc_csharp.transport.ITransport;
using MessageEventArgs = WebSocketSharp.MessageEventArgs;
using WebSocket = WebSocketSharp.WebSocket;

namespace RPC.Transports
{
    public class WebSocketClientTransport : WebSocket, ITransport
    {
        public WebSocketClientTransport(string url, params string[] protocols) : base(url, protocols)
        {
            this.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            base.OnMessage += this.HandleMessage;
            base.OnError += this.HandleError;
            base.OnClose += this.HandleClose;
            base.OnOpen += this.HandleOpen;

            base.Connect();
        }

        private void HandleMessage(object sender, MessageEventArgs e)
        {
            OnMessageEvent?.Invoke(e.RawData);
        }

        private void HandleError(object sender, ErrorEventArgs e)
        {
            OnErrorEvent?.Invoke(e.Message);
        }

        private void HandleClose(object sender, CloseEventArgs e)
        {
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

        public event Action OnCloseEvent;
        public event Action<string> OnErrorEvent;
        public event Action<byte[]> OnMessageEvent;
        public event Action OnConnectEvent;
    }
}
