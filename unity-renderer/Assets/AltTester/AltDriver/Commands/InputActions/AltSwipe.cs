namespace Altom.AltDriver.Commands
{
    public class AltSwipe : AltBaseCommand
    {
        AltSwipeParams cmdParams;
        public AltSwipe(IDriverCommunication commHandler, AltVector2 start, AltVector2 end, float duration, bool wait) : base(commHandler)
        {
            cmdParams = new AltSwipeParams(start, end, duration, wait);
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