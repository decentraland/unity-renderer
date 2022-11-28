namespace Altom.AltDriver.Commands
{
    public class AltMoveMouse : AltBaseCommand
    {
        AltMoveMouseParams cmdParams;
        public AltMoveMouse(IDriverCommunication commHandler, AltVector2 coordinates, float duration, bool wait) : base(commHandler)
        {
            cmdParams = new AltMoveMouseParams(coordinates, duration, wait);
        }
        public void Execute()
        {
            CommHandler.Send(cmdParams);
            var data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);
            if (cmdParams.wait)
            {
                data = CommHandler.Recvall<string>(cmdParams);
                ValidateResponse("Finished", data);
            }
        }
    }
}