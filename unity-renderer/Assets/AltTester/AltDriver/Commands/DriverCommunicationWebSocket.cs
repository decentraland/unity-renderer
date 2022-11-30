using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security.Permissions;
using System.Threading;
using Altom.AltDriver.Logging;
using Altom.AltDriver.Notifications;
using AltWebSocketSharp;
using Newtonsoft.Json;

namespace Altom.AltDriver.Commands
{
    public class DriverCommunicationWebSocket : IDriverCommunication
    {
        private static readonly NLog.Logger logger = DriverLogManager.Instance.GetCurrentClassLogger();
        private IWebSocketClient wsClient = null;
        private readonly string _host;
        private readonly int _port;
        private readonly string _uri;
        private readonly string _gameName;
        private readonly int _connectTimeout;
        private Queue<CommandResponse> messages;
        private List<Action<AltLoadSceneNotificationResultParams>> loadSceneCallbacks = new List<Action<AltLoadSceneNotificationResultParams>>();
        private List<Action<String>> unloadSceneCallbacks = new List<Action<String>>();
        private List<Action<AltLogNotificationResultParams>> logCallbacks = new List<Action<AltLogNotificationResultParams>>();
        private List<Action<bool>> applicationPausedCallbacks = new List<Action<bool>>();
        private List<string> messageIdTimeouts = new List<string>();

        private int commandTimeout = 60;
        private float delayAfterCommand = 0;

        public DriverCommunicationWebSocket(string host, int port, int connectTimeout, string gameName)
        {
            _host = host;
            _port = port;
            _gameName = gameName;

            _uri = "ws://" + host + ":" + port + "/altws?game=" + Uri.EscapeUriString(gameName);
            _connectTimeout = connectTimeout;

            messages = new Queue<CommandResponse>();
        }

        public void Connect()
        {
            int delay = 100;

            logger.Info("Connecting to: '{0}'.", _uri);

            WebSocket wsClient = new WebSocket(_uri);
            wsClient.OnError += (sender, args) =>
            {
                logger.Error(args.Exception, args.Message);
            };

            Stopwatch watch = Stopwatch.StartNew();
            int retries = 0;

            while (_connectTimeout > watch.Elapsed.TotalSeconds)
            {
                if (retries > 0)
                {
                    logger.Debug(string.Format("Retrying #{0} to connect to: '{1}'.", retries, _uri));
                }
                wsClient.Connect();

                if (wsClient.IsAlive) break;

                retries++;

                Thread.Sleep(delay); // delay between retries
            }
            if (watch.Elapsed.TotalSeconds > _connectTimeout && !wsClient.IsAlive)
                throw new ConnectionTimeoutException(string.Format("Failed to connect to AltTester on host: {0} port: {1}.", _host, _port));

            if (!wsClient.IsAlive)
                throw new ConnectionException(string.Format("Failed to connect to AltTester on host: {0} port: {1}.", _host, _port));

            logger.Debug("Connected to: " + _uri);

            this.wsClient = new AltWebSocketClient(wsClient);
            this.wsClient.OnMessage += OnMessage;
            this.wsClient.OnError += (sender, args) =>
            {
                logger.Error(args.Message);
                if (args.Exception != null)
                    logger.Error(args.Exception);
            };
            this.wsClient.OnClose += (sender, args) =>
            {
                logger.Debug("Connection to AltTester closed: [Code:{0}, Reason:{1}]", args.Code, args.Reason);
            };
        }

        public T Recvall<T>(CommandParams param)
        {
            Stopwatch watch = Stopwatch.StartNew();
            while (true)

            {

                while (messages.Count == 0 && wsClient.IsAlive() && commandTimeout >= watch.Elapsed.TotalSeconds)
                {
                    Thread.Sleep(10);
                }
                if (commandTimeout < watch.Elapsed.TotalSeconds && wsClient.IsAlive())
                {
                    messageIdTimeouts.Add(param.messageId);
                    throw new CommandResponseTimeoutException();
                }

                if (!wsClient.IsAlive())
                {
                    throw new AltException("Driver disconnected");
                }
                var message = messages.Dequeue();
                if (messageIdTimeouts.Contains(message.messageId))
                {
                    continue;
                }

                if ((message.error == null || message.error.type != AltErrors.errorInvalidCommand) && (message.messageId != param.messageId || message.commandName != param.commandName))
                {
                    throw new AltRecvallMessageIdException(string.Format("Response received does not match command send. Expected {0}:{1}. Got {2}:{3}", param.commandName, param.messageId, message.commandName, message.messageId));
                }
                handleErrors(message.error);
                if (message.data == null) return default(T);
                try
                {
                    return JsonConvert.DeserializeObject<T>(message.data);
                }
                catch (JsonReaderException)
                {
                    throw new ResponseFormatException(typeof(T), message.data);
                }
            }
        }


        public void Send(CommandParams param)
        {
            param.messageId = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            string message = JsonConvert.SerializeObject(param, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Culture = CultureInfo.InvariantCulture
            });
            this.wsClient.Send(message);
            logger.Debug("command sent: " + trimLog(message));
        }

        public void Close()
        {
            logger.Info(string.Format("Closing connection to AltTester on: {0}", _uri));
            this.wsClient.Close();
        }
        public void SetCommandTimeout(int timeout)
        {
            commandTimeout = timeout;
        }

        protected void OnMessage(object sender, string data)
        {
            var message = JsonConvert.DeserializeObject<CommandResponse>(data);

            if (message.isNotification)
            {
                handleNotification(message);
            }
            else
            {
                messages.Enqueue(message);
                logger.Debug("response received: " + trimLog(data));
            }

        }

        private void handleNotification(CommandResponse message)
        {
            handleErrors(message.error);
            switch (message.commandName)
            {
                case "loadSceneNotification":
                    AltLoadSceneNotificationResultParams data = JsonConvert.DeserializeObject<AltLoadSceneNotificationResultParams>(message.data);
                    foreach (var callback in loadSceneCallbacks)
                    {
                        callback(data);
                    }
                    break;
                case "unloadSceneNotification":
                    string sceneName = JsonConvert.DeserializeObject<string>(message.data);
                    foreach (var callback in unloadSceneCallbacks)
                    {
                        callback(sceneName);
                    }
                    break;
                case "logNotification":
                    AltLogNotificationResultParams data1 = JsonConvert.DeserializeObject<AltLogNotificationResultParams>(message.data);
                    foreach (var callback in logCallbacks)
                    {
                        callback(data1);
                    }
                    break;
                case "applicationPausedNotification":
                    bool data2 = JsonConvert.DeserializeObject<bool>(message.data);
                    foreach (var callback in applicationPausedCallbacks)
                    {
                        callback(data2);
                    }
                    break;
            }
        }

        private void handleErrors(CommandError error)
        {
            if (error == null)
            {
                return;
            }

            logger.Debug(error.type + ": " + error.message);
            logger.Debug(error.trace);

            switch (error.type)
            {
                case AltErrors.errorNotFound:
                    throw new NotFoundException(error.message);
                case AltErrors.errorSceneNotFound:
                    throw new SceneNotFoundException(error.message);
                case AltErrors.errorPropertyNotFound:
                    throw new PropertyNotFoundException(error.message);
                case AltErrors.errorMethodNotFound:
                    throw new MethodNotFoundException(error.message);
                case AltErrors.errorComponentNotFound:
                    throw new ComponentNotFoundException(error.message);
                case AltErrors.errorAssemblyNotFound:
                    throw new AssemblyNotFoundException(error.message);
                case AltErrors.errorCouldNotPerformOperation:
                    throw new CouldNotPerformOperationException(error.message);
                case AltErrors.errorMethodWithGivenParametersNotFound:
                    throw new MethodWithGivenParametersNotFoundException(error.message);
                case AltErrors.errorFailedToParseArguments:
                    throw new FailedToParseArgumentsException(error.message);
                case AltErrors.errorInvalidParameterType:
                    throw new InvalidParameterTypeException(error.message);
                case AltErrors.errorObjectWasNotFound:
                    throw new ObjectWasNotFoundException(error.message);
                case AltErrors.errorPropertyNotSet:
                    throw new PropertyNotFoundException(error.message);
                case AltErrors.errorNullReference:
                    throw new NullReferenceException(error.message);
                case AltErrors.errorUnknownError:
                    throw new UnknownErrorException(error.message);
                case AltErrors.errorFormatException:
                    throw new FormatException(error.message);
                case AltErrors.errorInvalidPath:
                    throw new InvalidPathException(error.message);
                case AltErrors.errorInvalidCommand:
                    throw new InvalidCommandException(error.message);
                case AltErrors.errorInputModule:
                    throw new AltInputModuleException(error.message);
                case AltErrors.errorCameraNotFound:
                    throw new AltCameraNotFoundException(error.message);
            }

            logger.Debug(error.type + " is not handled by driver.");
            throw new UnknownErrorException(error.message);
        }
        private string trimLog(string log, int maxLogLength = 1000)
        {
            if (string.IsNullOrEmpty(log)) return log;
            if (log.Length <= maxLogLength) return log;
            return log.Substring(0, maxLogLength) + "[...]";
        }

        public void AddNotificationListener<T>(NotificationType notificationType, Action<T> callback, bool overwrite)
        {
            switch (notificationType)
            {
                case NotificationType.LOADSCENE:
                    if (callback.GetType() != typeof(Action<AltLoadSceneNotificationResultParams>))
                    {
                        throw new InvalidCastException(String.Format("callback must be of type: {0} and not {1}", typeof(Action<AltLoadSceneNotificationResultParams>), callback.GetType()));
                    }
                    if (overwrite)
                        loadSceneCallbacks.Clear();
                    loadSceneCallbacks.Add(callback as Action<AltLoadSceneNotificationResultParams>);
                    break;
                case NotificationType.UNLOADSCENE:
                    if (callback.GetType() != typeof(Action<String>))
                    {
                        throw new InvalidCastException(String.Format("callback must be of type: {0} and not {1}", typeof(Action<String>), callback.GetType()));
                    }
                    if (overwrite)
                        unloadSceneCallbacks.Clear();
                    unloadSceneCallbacks.Add(callback as Action<String>);
                    break;
                case NotificationType.LOG:
                    if (callback.GetType() != typeof(Action<AltLogNotificationResultParams>))
                    {
                        throw new InvalidCastException(String.Format("callback must be of type: {0} and not {1}", typeof(Action<AltLogNotificationResultParams>), callback.GetType()));
                    }
                    if (overwrite)
                        logCallbacks.Clear();
                    logCallbacks.Add(callback as Action<AltLogNotificationResultParams>);
                    break;
                case NotificationType.APPLICATION_PAUSED:
                    if (callback.GetType() != typeof(Action<bool>))
                    {
                        throw new InvalidCastException(String.Format("callback must be of type: {0} and not {1}", typeof(Action<bool>), callback.GetType()));
                    }
                    if (overwrite)
                        applicationPausedCallbacks.Clear();
                    applicationPausedCallbacks.Add(callback as Action<bool>);
                    break;
            }
        }

        public void RemoveNotificationListener(NotificationType notificationType)
        {
            switch (notificationType)
            {
                case NotificationType.LOADSCENE:
                    loadSceneCallbacks.Clear();
                    break;
                case NotificationType.UNLOADSCENE:
                    unloadSceneCallbacks.Clear();
                    break;
                case NotificationType.LOG:
                    logCallbacks.Clear();
                    break;
                case NotificationType.APPLICATION_PAUSED:
                    applicationPausedCallbacks.Clear();
                    break;
            }
        }
        public void SetDelayAfterCommand(float delay)
        {
            delayAfterCommand = delay;
        }

        public float GetDelayAfterCommand()
        {
            return delayAfterCommand;
        }

        public void SleepFor(float time)
        {
            Thread.Sleep(System.Convert.ToInt32(time * 1000));
        }
    }
}
