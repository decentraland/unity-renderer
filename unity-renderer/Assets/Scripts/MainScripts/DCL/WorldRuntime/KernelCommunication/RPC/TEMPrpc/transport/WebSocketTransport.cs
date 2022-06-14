using System;
using rpc_csharp.transport;
using WebSocketSharp.Server;

public class WebSocketServerTransport : WebSocketBehavior, ITransport
{
    protected override void OnMessage(WebSocketSharp.MessageEventArgs e)
    {
        base.OnMessage(e);
        OnMessageEvent?.Invoke(e.RawData);
    }

    protected override void OnError(WebSocketSharp.ErrorEventArgs e)
    {
        base.OnError(e);
        OnErrorEvent?.Invoke(e.Message);
    }

    protected override void OnClose(WebSocketSharp.CloseEventArgs e)
    {
        base.OnClose(e);
        OnCloseEvent?.Invoke();
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        OnConnectEvent?.Invoke();
    }

    public void SendMessage(byte[] data)
    {
        Send(data);
    }

    public void Close()
    {
        Sessions.CloseSession(ID);
    }

    public event Action OnCloseEvent;
    public event Action<string> OnErrorEvent;
    public event Action<byte[]> OnMessageEvent;
    public event Action OnConnectEvent;
}