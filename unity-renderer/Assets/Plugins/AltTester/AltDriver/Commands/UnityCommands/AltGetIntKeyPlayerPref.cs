namespace Altom.AltDriver.Commands
{
    public class AltGetIntKeyPlayerPref : AltBaseCommand
    {
        readonly AltGetKeyPlayerPrefParams cmdParams;
        public AltGetIntKeyPlayerPref(IDriverCommunication commHandler, string keyName) : base(commHandler)
        {
            cmdParams = new AltGetKeyPlayerPrefParams(keyName, PlayerPrefKeyType.Int);
        }
        public int Execute()
        {
            CommHandler.Send(cmdParams);
            return CommHandler.Recvall<int>(cmdParams);
        }
    }
}