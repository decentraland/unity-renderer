namespace Altom.AltDriver.Commands
{
    public class AltScroll : AltBaseCommand
    {
        AltScrollParams cmdParams;
        public AltScroll(IDriverCommunication commHandler, float speed, float speedHorizontal, float duration, bool wait) : base(commHandler)
        {
            cmdParams = new AltScrollParams(speed, duration, wait, speedHorizontal);
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