namespace Altom.AltDriver.Commands
{
    public class AltMultipointSwipe : AltBaseCommand
    {
        AltMultipointSwipeParams cmdParams;

        public AltMultipointSwipe(IDriverCommunication commHandler, AltVector2[] positions, float duration, bool wait) : base(commHandler)
        {
            cmdParams = new AltMultipointSwipeParams(positions, duration, wait);
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
