using System;

namespace Altom.AltTester.Communication
{
    public delegate void SendMessageHandler(string message);
    public delegate void CommunicationHandler();
    public delegate void CommunicationErrorHandler(string message, Exception error);
    public interface ICommunication
    {
        CommunicationHandler OnConnect { get; set; }
        CommunicationHandler OnDisconnect { get; set; }
        CommunicationErrorHandler OnError { get; set; }
        bool IsConnected { get; }
        bool IsListening { get; }
        void Start();
        void Stop();
    }

    public class UnhandledStartCommError : Exception
    {
        public UnhandledStartCommError(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
    public class AddressInUseCommError : Exception
    {
        public AddressInUseCommError(string message) : base(message)
        {

        }
    }

}