using AltWebSocketSharp;
using UnityEngine.Playables;

namespace Altom.AltTester.Communication
{
    public class AltClientWebSocketHandler : BaseWebSocketHandler
    {
        private readonly WebSocket _webSocket;


        public AltClientWebSocketHandler(WebSocket webSocket, ICommandHandler commandHandler) : base(commandHandler)
        {
            this._webSocket = webSocket;
            webSocket.OnMessage += this.onMessage;


            this._commandHandler.OnSendMessage += webSocket.Send;
        }

        private void onMessage(object sender, MessageEventArgs message)
        {
            this._commandHandler.OnMessage(message.Data);
        }
    }
}