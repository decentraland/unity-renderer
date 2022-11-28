namespace Altom.AltDriver.Commands
{
    public class AltEndTouch : AltBaseCommand
    {
        AltEndTouchParams cmdParams;

        public AltEndTouch(IDriverCommunication commHandler, int fingerId) : base(commHandler)
        {
            this.cmdParams = new AltEndTouchParams(fingerId);
        }
        public void Execute()
        {
            CommHandler.Send(cmdParams);
            var data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);
        }
    }
}