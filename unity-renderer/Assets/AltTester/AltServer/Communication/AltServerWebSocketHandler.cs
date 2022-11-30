using Altom.AltTester.Logging;
using AltWebSocketSharp;
using AltWebSocketSharp.Server;

namespace Altom.AltTester.Communication
{
    public class AltServerWebSocketHandler : WebSocketBehavior
    {
        private static readonly NLog.Logger logger = ServerLogManager.Instance.GetCurrentClassLogger();
        private ICommandHandler commandHandler;

        public CommunicationErrorHandler OnErrorHandler;
        public CommunicationHandler OnClientConnected;
        public CommunicationHandler OnClientDisconnected;

        public AltServerWebSocketHandler()
        {

        }

        public void Init(ICommandHandler cmdHandler)
        {
            this.commandHandler = cmdHandler;
            commandHandler.OnSendMessage += this.Send;
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            logger.Debug("Client " + this.ID + " connected.");
            if (OnClientConnected != null) OnClientConnected.Invoke();
        }

        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
            logger.Debug("Client " + this.ID + " disconnected.");
            if (OnClientDisconnected != null) OnClientDisconnected.Invoke();
            commandHandler.OnSendMessage -= this.Send;
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
            if (OnErrorHandler != null)
                OnErrorHandler.Invoke(e.Message, e.Exception);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
            commandHandler.OnMessage(e.Data);
        }
    }
}