using System;
using System.Runtime.InteropServices;
using AOT;
using rpc_csharp.transport;

namespace RPC.Transports
{
    public class WebGLTransport : ITransport
    {
        public event Action OnCloseEvent;

        public event Action<string> OnErrorEvent;

        public event Action<byte[]> OnMessageEvent;

        public event Action OnConnectEvent;

        public WebGLTransport()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SetCallback_BinaryMessage(BinaryMessage);
#endif
            OnWebGLMessage += OnWebgGLMessageReceived;
        }

        public void SendMessage(byte[] data)
        {
            BinaryMessageFromEngine(data, data.Length);
        }

        public void Close()
        {
            OnWebGLMessage -= OnWebgGLMessageReceived;
        }

        private void OnWebgGLMessageReceived(byte[] data)
        {
            OnMessageEvent?.Invoke(data);
        }

        delegate void JS_Delegate_VII(int a, int b);

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] public static extern void BinaryMessageFromEngine(byte[] bytes, int size);
#else
        private static void BinaryMessageFromEngine(byte[] bytes, int size) { }
#endif

        private static event Action<byte[]> OnWebGLMessage;

        [DllImport("__Internal")]
        private static extern void SetCallback_BinaryMessage(JS_Delegate_VII callback);

        [MonoPInvokeCallback(typeof(JS_Delegate_VII))]
        internal static void BinaryMessage(int intPtr, int length)
        {
            byte[] data = new byte[length];
            Marshal.Copy(new IntPtr(intPtr), data, 0, length);
            OnWebGLMessage?.Invoke(data);
        }
    }
}