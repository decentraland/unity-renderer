using System;
using rpc_csharp.transport;

namespace RPC.Transports
{
    public class WebSocketTransport : ITransport
    {
        public event Action OnCloseEvent;

        public event Action<string> OnErrorEvent;

        public event Action<byte[]> OnMessageEvent;

        public event Action OnConnectEvent;

        private DCLWebSocketService wsService;

        public WebSocketTransport()
        {
            // TODO: refactor websocket service to avoid the need of this reference
            if (WebSocketCommunication.service != null)
            {
                OnWebSocketServiceAdded(WebSocketCommunication.service);
            }
            else
            {
                WebSocketCommunication.OnWebSocketServiceAdded += OnWebSocketServiceAdded;
            }
        }

        public void SendMessage(byte[] data)
        {
            wsService?.SendBinary(data);
        }

        public void Close()
        {
            WebSocketCommunication.OnWebSocketServiceAdded -= OnWebSocketServiceAdded;

            if (wsService != null)
            {
                wsService.OnConnectEvent -= OnConnect;
                wsService.OnMessageEvent -= OnMessage;
                wsService.OnErrorEvent -= OnError;
                wsService.OnCloseEvent -= OnClose;
            }
        }

        private void OnWebSocketServiceAdded(DCLWebSocketService service)
        {
            WebSocketCommunication.OnWebSocketServiceAdded -= OnWebSocketServiceAdded;
            wsService = service;

            wsService.OnConnectEvent += OnConnect;
            wsService.OnMessageEvent += OnMessage;
            wsService.OnErrorEvent += OnError;
            wsService.OnCloseEvent += OnClose;
        }

        private void OnClose()
        {
            OnCloseEvent?.Invoke();
        }

        private void OnError(string error)
        {
            OnErrorEvent?.Invoke(error);
        }

        private void OnMessage(byte[] message)
        {
            OnMessageEvent?.Invoke(message);
        }

        private void OnConnect()
        {
            OnConnectEvent?.Invoke();
        }
    }
}