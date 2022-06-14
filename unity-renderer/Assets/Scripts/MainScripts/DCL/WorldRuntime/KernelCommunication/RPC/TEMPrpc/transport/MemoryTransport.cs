using System;

namespace rpc_csharp.transport
{
    public class MemoryTransport : ITransport
    {
        private MemoryTransport sender;

        private MemoryTransport()
        {
            
        }

        public static (ITransport, ITransport) Create()
        {
            var client = new MemoryTransport();
            var server = new MemoryTransport();
                
            client.Attach(server);
            server.Attach(client);

            return (client, server);
        }

        public void Attach(MemoryTransport sender)
        {
            this.sender = sender;
        }
        public Action<byte[]> GetOnMessageEvent()
        {
            return OnMessageEvent;
        }
            
        public Action GetOnCloseEvent()
        {
            return OnCloseEvent;
        }
        public void SendMessage(byte[] data)
        {
            sender.GetOnMessageEvent().Invoke(data);
        }

        public void Close()
        {
            sender.GetOnCloseEvent().Invoke();
        }

        public event Action OnCloseEvent;
        public event Action<string> OnErrorEvent;
        public event Action<byte[]> OnMessageEvent;
        public event Action OnConnectEvent;
    }
}