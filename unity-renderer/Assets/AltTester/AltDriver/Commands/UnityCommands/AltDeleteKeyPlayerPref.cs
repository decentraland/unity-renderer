namespace Altom.AltDriver.Commands
{
    public class AltDeleteKeyPlayerPref : AltBaseCommand
    {
        AltDeleteKeyPlayerPrefParams cmdParams;
        public AltDeleteKeyPlayerPref(IDriverCommunication commHandler, string keyName) : base(commHandler)
        {
            this.cmdParams = new AltDeleteKeyPlayerPrefParams(keyName);
        }
        public void Execute()
        {
            CommHandler.Send(cmdParams);
            var data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);
        }
    }
}