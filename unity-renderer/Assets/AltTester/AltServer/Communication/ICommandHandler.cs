namespace Altom.AltTester.Communication
{
    public interface ICommandHandler
    {
        SendMessageHandler OnSendMessage { get; set; }
        void Send(string data);
        void OnMessage(string data);
    }
}