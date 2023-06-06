using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;
using WebSocketSharp;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;
using ITransport = rpc_csharp.transport.ITransport;
using MessageEventArgs = WebSocketSharp.MessageEventArgs;
using WebSocket = WebSocketSharp.WebSocket;

namespace RPC.Transports
{
    public class WebSocketClientTransport : WebSocket, ITransport
    {
        private CancellationTokenSource pingOverTimeCancellationToken = new ();

        public event Action OnCloseEvent;
        public event Action<string> OnErrorEvent;
        public event Action<byte[]> OnMessageEvent;
        public event Action OnConnectEvent;

        public WebSocketClientTransport(string url, params string[] protocols) : base(url, protocols)
        {
            Debug.Log("WebSocketClientTransport.Constructor.Pre");
            this.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;

            base.OnMessage += this.HandleMessage;
            base.OnError += this.HandleError;
            base.OnClose += this.HandleClose;
            base.OnOpen += this.HandleOpen;

            Debug.Log("WebSocketClientTransport.Constructor.Connect.Pre");
            base.Connect();
            Debug.Log("WebSocketClientTransport.Constructor.Connect.Post");
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
            pingOverTimeCancellationToken.SafeCancelAndDispose();
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

        public void KeepConnectionAlive(TimeSpan signalFrequency, int failureTimesForCancelling)
        {
            async UniTaskVoid WaitUntilIsConnectedAndPingOverTime(CancellationToken cancellationToken)
            {
                while (!IsAlive)
                    await UniTask.Delay(signalFrequency, cancellationToken: cancellationToken);

                var pingFailedTimes = 0;

                while (IsAlive && pingFailedTimes < failureTimesForCancelling)
                {
                    await UniTask.Delay(signalFrequency, cancellationToken: cancellationToken);

                    if (Ping())
                        pingFailedTimes = 0;
                    else
                        pingFailedTimes++;
                }
            }

            pingOverTimeCancellationToken = pingOverTimeCancellationToken.SafeRestart();
            WaitUntilIsConnectedAndPingOverTime(pingOverTimeCancellationToken.Token).Forget();
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
    }
}
