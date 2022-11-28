using System;
using Altom.AltTester.Logging;
using AltWebSocketSharp.Server;

namespace Altom.AltTester.Communication
{
    public class WebSocketServerCommunication : ICommunication
    {
        private static readonly NLog.Logger logger = ServerLogManager.Instance.GetCurrentClassLogger();

        private readonly int port;
        private readonly string host;

        WebSocketServer wsServer;
        AltServerWebSocketHandler wsHandler = null;

        public WebSocketServerCommunication(ICommandHandler cmdHandler, string host, int port)
        {
            this.port = port;
            this.host = host;
            Uri uri;
            if (!Uri.TryCreate(string.Format("ws://{0}:{1}/", host, port), UriKind.Absolute, out uri))
            {
                throw new Exception(String.Format("Invalid host or port {0}:{1}", host, port));
            }

            wsServer = new WebSocketServer(uri.ToString());
            wsServer.AllowForwardedRequest = true;

            wsServer.AddWebSocketService<AltServerWebSocketHandler>("/altws", (context, handler) =>
            {
                if (wsServer.WebSocketServices["/altws"].Sessions.Count == 1)
                {
                    throw new Exception("Driver already connected.");
                }

                handler.Init(cmdHandler);
                this.wsHandler = handler;

                this.wsHandler.OnErrorHandler += (message, exception) =>
                {
                    if (this.OnError != null) this.OnError.Invoke(message, exception);
                };

                this.wsHandler.OnClientConnected += () =>
                {
                    if (this.OnConnect != null) this.OnConnect.Invoke();
                };

                this.wsHandler.OnClientDisconnected += () =>
                {
                    if (this.OnDisconnect != null)
                    {
                        if (wsServer.WebSocketServices["/altws"].Sessions.Count == 0)
                        {
                            this.OnDisconnect();
                        }
                    }
                };
            });
        }

        public bool IsConnected { get { return wsServer.WebSocketServices["/altws"].Sessions.Count > 0; } }
        public bool IsListening { get { return wsServer.IsListening; } }

        public CommunicationHandler OnConnect { get; set; }
        public CommunicationHandler OnDisconnect { get; set; }
        public CommunicationErrorHandler OnError { get; set; }

        public void Start()
        {
            try
            {
                if (!wsServer.IsListening)
                {
                    wsServer.Start();
                }
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException && ex.InnerException != null && (ex.InnerException.Message.Contains("Only one usage of each socket address") || ex.InnerException.Message.Contains("Address already in use")))
                {
                    string message = String.Format("Port {0} is in use by another program. Start AltTester with a different port.", port);

                    throw new AddressInUseCommError(message);
                }

                logger.Error(ex.GetType().ToString(), ex.InnerException.Message);
                throw new UnhandledStartCommError("An unexpected error occurred while starting AltTester.", ex);
            }
        }

        public void Stop()
        {
            wsServer.Stop();
        }
    }
}