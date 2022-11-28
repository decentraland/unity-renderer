namespace Altom.AltDriver.Commands
{
    public class AltDeletePlayerPref : AltBaseCommand
    {
        AltDeletePlayerPrefParams cmdParams;
        public AltDeletePlayerPref(IDriverCommunication commHandler) : base(commHandler)
        {
            this.cmdParams = new AltDeletePlayerPrefParams();
        }
        public void Execute()
        {
            CommHandler.Send(cmdParams);
            var data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);
        }
    }
}