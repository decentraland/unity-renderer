using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;

namespace Altom.AltTester.Communication
{
#if UNITY_WEBGL
    public enum WebSocketCloseCode
    {
        /* Do NOT use NotSet - it's only purpose is to indicate that the close code cannot be parsed. */
        NotSet = 0,
        Normal = 1000,
        Away = 1001,
        ProtocolError = 1002,
        UnsupportedData = 1003,
        Undefined = 1004,
        NoStatus = 1005,
        Abnormal = 1006,
        InvalidData = 1007,
        PolicyViolation = 1008,
        TooBig = 1009,
        MandatoryExtension = 1010,
        ServerError = 1011,
        TlsHandshakeFailure = 1015
    }

    public enum WebSocketState
    {
        Connecting,
        Open,
        Closing,
        Closed
    }

    public delegate void WebSocketOpenEventHandler();
    public delegate void WebSocketMessageEventHandler(byte[] data);
    public delegate void WebSocketErrorEventHandler(string errorMsg);
    public delegate void WebSocketCloseEventHandler(WebSocketCloseCode closeCode);

    public class WebSocketWebGLCommunication : ICommunication
    {
        WebGLWebSocket webglWebSocket;
        public WebSocketWebGLCommunication(ICommandHandler cmdHandler, string host, int port)
        {
            Uri uri;
            if (!Uri.TryCreate(string.Format("ws://{0}:{1}/altws/game", host, port), UriKind.Absolute, out uri))
            {
                throw new Exception(String.Format("Invalid host or port {0}:{1}", host, port));
            }

            webglWebSocket = new WebGLWebSocket(uri.ToString());

            var webGLWebSocketHandler = new AltWebGLWebSocketHandler(cmdHandler, webglWebSocket);

            webglWebSocket.OnOpen += () =>
            {
                if (this.OnConnect != null) this.OnConnect.Invoke();
                webglWebSocket.OnError += (string errorMsg) =>
                {
                    if (this.OnError != null) this.OnError.Invoke(errorMsg, null);
                };
            };

            webglWebSocket.OnClose += (WebSocketCloseCode closeCode) =>
            {
                if (this.OnDisconnect != null) this.OnDisconnect.Invoke();
            };
        }
        public bool IsConnected => webglWebSocket.State == WebSocketState.Open;
        public bool IsListening => false;

        public CommunicationHandler OnConnect { get; set; }
        public CommunicationHandler OnDisconnect { get; set; }
        public CommunicationErrorHandler OnError { get; set; }

        public void Start()
        {
            try
            {
                webglWebSocket.Connect().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw new UnhandledStartCommError("An error occurred while starting client CommunicationProtocol", ex);
            }
        }

        public void Stop()
        {
            webglWebSocket.Close().GetAwaiter().GetResult();
        }

    }
    public class WebGLWebSocket
    {
        /* WebSocket JSLIB functions */
        [DllImport("__Internal")]
        public static extern int WebSocketConnect(int instanceId);

        [DllImport("__Internal")]
        public static extern int WebSocketClose(int instanceId, int code, string reason);

        [DllImport("__Internal")]
        public static extern int WebSocketSend(int instanceId, byte[] dataPtr, int dataLength);

        [DllImport("__Internal")]
        public static extern int WebSocketSendText(int instanceId, string message);

        [DllImport("__Internal")]
        public static extern int WebSocketGetState(int instanceId);

        protected int InstanceId;

        public event WebSocketOpenEventHandler OnOpen;
        public event WebSocketMessageEventHandler OnMessage;
        public event WebSocketErrorEventHandler OnError;
        public event WebSocketCloseEventHandler OnClose;

        public WebGLWebSocket(string url, Dictionary<string, string> headers = null)
        {
            if (!WebSocketFactory.isInitialized)
            {
                WebSocketFactory.Initialize();
            }

            int instanceId = WebSocketFactory.WebSocketAllocate(url);
            WebSocketFactory.instances.Add(instanceId, this);

            this.InstanceId = instanceId;
        }

        ~WebGLWebSocket()
        {
            WebSocketFactory.HandleInstanceDestroy(this.InstanceId);
        }

        public int GetInstanceId()
        {
            return this.InstanceId;
        }

        public Task Connect()
        {
            int ret = WebSocketConnect(this.InstanceId);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);

            return Task.CompletedTask;
        }

        public void CancelConnection()
        {
            if (State == WebSocketState.Open)
                Close(WebSocketCloseCode.Abnormal);
        }

        public Task Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {
            int ret = WebSocketClose(this.InstanceId, (int)code, reason);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);

            return Task.CompletedTask;
        }

        public Task Send(byte[] data)
        {
            int ret = WebSocketSend(this.InstanceId, data, data.Length);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);

            return Task.CompletedTask;
        }

        public Task SendText(string message)
        {
            int ret = WebSocketSendText(this.InstanceId, message);

            if (ret < 0)
                throw WebSocketHelpers.GetErrorMessageFromCode(ret, null);

            return Task.CompletedTask;
        }

        public WebSocketState State
        {
            get
            {
                int state = WebSocketGetState(this.InstanceId);

                if (state < 0)
                    throw WebSocketHelpers.GetErrorMessageFromCode(state, null);

                switch (state)
                {
                    case 0:
                        return WebSocketState.Connecting;

                    case 1:
                        return WebSocketState.Open;

                    case 2:
                        return WebSocketState.Closing;

                    case 3:
                        return WebSocketState.Closed;

                    default:
                        return WebSocketState.Closed;
                }
            }
        }

        public void DelegateOnOpenEvent()
        {
            this.OnOpen?.Invoke();
        }

        public void DelegateOnMessageEvent(byte[] data)
        {
            this.OnMessage?.Invoke(data);
        }

        public void DelegateOnErrorEvent(string errorMsg)
        {
            this.OnError?.Invoke(errorMsg);
        }

        public void DelegateOnCloseEvent(int closeCode)
        {
            this.OnClose?.Invoke(WebSocketHelpers.ParseCloseCodeEnum(closeCode));
        }
    }

    /// <summary>
    /// Class providing static access methods to work with JSLIB WebSocket or WebSocketSharp interface
    /// </summary>
    public static class WebSocketFactory
    {

        /* Map of websocket instances */
        public static Dictionary<Int32, WebGLWebSocket> instances = new Dictionary<Int32, WebGLWebSocket>();

        /* Delegates */
        public delegate void OnOpenCallback(int instanceId);
        public delegate void OnMessageCallback(int instanceId, System.IntPtr msgPtr, int msgSize);
        public delegate void OnErrorCallback(int instanceId, System.IntPtr errorPtr);
        public delegate void OnCloseCallback(int instanceId, int closeCode);

        /* WebSocket JSLIB callback setters and other functions */
        [DllImport("__Internal")]
        public static extern int WebSocketAllocate(string url);

        [DllImport("__Internal")]
        public static extern void WebSocketFree(int instanceId);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnOpen(OnOpenCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnMessage(OnMessageCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnError(OnErrorCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnClose(OnCloseCallback callback);

        /* If callbacks was initialized and set */
        public static bool isInitialized = false;

        /*
        * Initialize WebSocket callbacks to JSLIB
        */
        public static void Initialize()
        {

            WebSocketSetOnOpen(DelegateOnOpenEvent);
            WebSocketSetOnMessage(DelegateOnMessageEvent);
            WebSocketSetOnError(DelegateOnErrorEvent);
            WebSocketSetOnClose(DelegateOnCloseEvent);

            isInitialized = true;
        }

        /// <summary>
        /// Called when instance is destroyed (by destructor)
        /// Method removes instance from map and free it in JSLIB implementation
        /// </summary>
        /// <param name="instanceId">Instance identifier.</param>
        public static void HandleInstanceDestroy(int instanceId)
        {

            instances.Remove(instanceId);
            WebSocketFree(instanceId);

        }

        [MonoPInvokeCallback(typeof(OnOpenCallback))]
        public static void DelegateOnOpenEvent(int instanceId)
        {

            WebGLWebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {
                instanceRef.DelegateOnOpenEvent();
            }

        }

        [MonoPInvokeCallback(typeof(OnMessageCallback))]
        public static void DelegateOnMessageEvent(int instanceId, System.IntPtr msgPtr, int msgSize)
        {

            WebGLWebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {
                byte[] msg = new byte[msgSize];
                Marshal.Copy(msgPtr, msg, 0, msgSize);

                instanceRef.DelegateOnMessageEvent(msg);
            }

        }

        [MonoPInvokeCallback(typeof(OnErrorCallback))]
        public static void DelegateOnErrorEvent(int instanceId, System.IntPtr errorPtr)
        {

            WebGLWebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {

                string errorMsg = Marshal.PtrToStringAuto(errorPtr);
                instanceRef.DelegateOnErrorEvent(errorMsg);

            }

        }

        [MonoPInvokeCallback(typeof(OnCloseCallback))]
        public static void DelegateOnCloseEvent(int instanceId, int closeCode)
        {

            WebGLWebSocket instanceRef;

            if (instances.TryGetValue(instanceId, out instanceRef))
            {
                instanceRef.DelegateOnCloseEvent(closeCode);
            }

        }
    }

    public static class WebSocketHelpers
    {
        public static WebSocketCloseCode ParseCloseCodeEnum(int closeCode)
        {

            if (WebSocketCloseCode.IsDefined(typeof(WebSocketCloseCode), closeCode))
            {
                return (WebSocketCloseCode)closeCode;
            }
            else
            {
                return WebSocketCloseCode.Undefined;
            }

        }

        public static WebSocketException GetErrorMessageFromCode(int errorCode, Exception inner)
        {
            switch (errorCode)
            {
                case -1:
                    return new WebSocketUnexpectedException("WebSocket instance not found.", inner);
                case -2:
                    return new WebSocketInvalidStateException("WebSocket is already connected or in connecting state.", inner);
                case -3:
                    return new WebSocketInvalidStateException("WebSocket is not connected.", inner);
                case -4:
                    return new WebSocketInvalidStateException("WebSocket is already closing.", inner);
                case -5:
                    return new WebSocketInvalidStateException("WebSocket is already closed.", inner);
                case -6:
                    return new WebSocketInvalidStateException("WebSocket is not in open state.", inner);
                case -7:
                    return new WebSocketInvalidArgumentException("Cannot close WebSocket. An invalid code was specified or reason is too long.", inner);
                default:
                    return new WebSocketUnexpectedException("Unknown error.", inner);
            }
        }
    }

    public class WebSocketException : Exception
    {
        public WebSocketException() { }
        public WebSocketException(string message) : base(message) { }
        public WebSocketException(string message, Exception inner) : base(message, inner) { }
    }

    public class WebSocketUnexpectedException : WebSocketException
    {
        public WebSocketUnexpectedException() { }
        public WebSocketUnexpectedException(string message) : base(message) { }
        public WebSocketUnexpectedException(string message, Exception inner) : base(message, inner) { }
    }

    public class WebSocketInvalidArgumentException : WebSocketException
    {
        public WebSocketInvalidArgumentException() { }
        public WebSocketInvalidArgumentException(string message) : base(message) { }
        public WebSocketInvalidArgumentException(string message, Exception inner) : base(message, inner) { }
    }

    public class WebSocketInvalidStateException : WebSocketException
    {
        public WebSocketInvalidStateException() { }
        public WebSocketInvalidStateException(string message) : base(message) { }
        public WebSocketInvalidStateException(string message, Exception inner) : base(message, inner) { }
    }
#endif
}