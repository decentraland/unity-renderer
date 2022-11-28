namespace Altom.AltDriver.Commands
{
    public class AltMoveTouch : AltBaseCommand
    {
        AltMoveTouchParams cmdParams;

        public AltMoveTouch(IDriverCommunication commHandler, int fingerId, AltVector2 coordinates) : base(commHandler)
        {
            this.cmdParams = new AltMoveTouchParams(fingerId, coordinates);
        }
        public void Execute()
        {
            CommHandler.Send(cmdParams);
            var data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);
        }
    }
}