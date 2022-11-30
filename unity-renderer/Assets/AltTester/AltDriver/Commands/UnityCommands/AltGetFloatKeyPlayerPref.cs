namespace Altom.AltDriver.Commands
{
    public class AltGetFloatKeyPlayerPref : AltBaseCommand
    {
        readonly AltGetKeyPlayerPrefParams cmdParams;
        public AltGetFloatKeyPlayerPref(IDriverCommunication commHandler, string keyName) : base(commHandler)
        {
            cmdParams = new AltGetKeyPlayerPrefParams(keyName, PlayerPrefKeyType.Float);
        }
        public float Execute()
        {
            CommHandler.Send(cmdParams);
            return CommHandler.Recvall<float>(cmdParams);
        }
    }
}