using System;

namespace rpc_csharp.transport
{
    public interface ITransport
    {
        // Functions
        void SendMessage(byte[] data);

        void Close();
        
        // Events
        event Action OnCloseEvent;

        event Action<string> OnErrorEvent;

        event Action<byte[]> OnMessageEvent;

        event Action OnConnectEvent;
    }
}