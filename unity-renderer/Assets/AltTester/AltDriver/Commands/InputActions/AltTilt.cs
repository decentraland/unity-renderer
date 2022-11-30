namespace Altom.AltDriver.Commands
{
    public class AltTilt : AltBaseCommand
    {
        AltTiltParams cmdParams;
        public AltTilt(IDriverCommunication commHandler, AltVector3 acceleration, float duration, bool wait) : base(commHandler)
        {
            cmdParams = new AltTiltParams(acceleration, duration, wait);
        }
        public void Execute()
        {
            CommHandler.Send(cmdParams);
            string data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);
            if (cmdParams.wait)
            {
                data = CommHandler.Recvall<string>(cmdParams);
                ValidateResponse("Finished", data);
            }
        }
    }
}