namespace Altom.AltDriver.Commands
{
    public class AltTapCoordinates : AltBaseCommand
    {
        AltTapCoordinatesParams cmdParams;
        public AltTapCoordinates(IDriverCommunication commHandler, AltVector2 coordinates, int count, float interval, bool wait) : base(commHandler)
        {
            cmdParams = new AltTapCoordinatesParams(coordinates, count, interval, wait);
        }
        public void Execute()
        {
            CommHandler.Send(cmdParams);
            string data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);
            if (cmdParams.wait)
            {
                data = CommHandler.Recvall<string>(cmdParams); ;
                ValidateResponse("Finished", data);
            }
        }
    }
}