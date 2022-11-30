using System;
using AltWebSocketSharp;

namespace Altom.AltDriver.Commands
{
    public class AltWebSocketClient : IWebSocketClient
    {
        private readonly WebSocket webSocket;
        public AltWebSocketClient(WebSocket webSocket)
        {
            this.webSocket = webSocket;
            this.webSocket.OnMessage += (sender, message) => this.OnMessage.Invoke(this, message.Data);
            this.webSocket.OnError += (sender, error) => this.OnError.Invoke(this, error);
            this.webSocket.OnClose += (sender, closeEventArgs) => this.OnClose.Invoke(this, closeEventArgs);
        }

        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<string> OnMessage;
        public event EventHandler<CloseEventArgs> OnClose;


        public void Close()
        {
            this.webSocket.Close();
        }

        public void Send(string data)
        {
            this.webSocket.Send(data);
        }
        public bool IsAlive()
        {
            return this.webSocket.IsAlive;
        }
    }
}