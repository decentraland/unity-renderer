namespace Altom.AltDriver.Commands
{
    public class AltGetStringKeyPlayerPref : AltBaseCommand
    {
        readonly AltGetKeyPlayerPrefParams cmdParams;
        public AltGetStringKeyPlayerPref(IDriverCommunication commHandler, string keyName) : base(commHandler)
        {
            cmdParams = new AltGetKeyPlayerPrefParams(keyName, PlayerPrefKeyType.String);
        }
        public string Execute()
        {
            CommHandler.Send(cmdParams);
            return CommHandler.Recvall<string>(cmdParams);
        }
    }
}