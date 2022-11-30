using System.Text;

namespace Altom.AltTester.Communication
{
#if UNITY_WEBGL
    public class AltWebGLWebSocketHandler : BaseWebSocketHandler
    {
        protected readonly WebGLWebSocket _webSocket;

        public AltWebGLWebSocketHandler(ICommandHandler cmdHandler, WebGLWebSocket webSocket) : base(cmdHandler)
        {
            this._webSocket = webSocket;
            this._webSocket.OnMessage += this.onMessage;

            _commandHandler.OnSendMessage += (message) =>
             {
                 this._webSocket.SendText(message).ConfigureAwait(false).GetAwaiter().GetResult();
             };



        }
        private void onMessage(byte[] data)
        {
            var message = Encoding.UTF8.GetString(data);
            this._commandHandler.OnMessage(message);
        }
    }
#endif
}