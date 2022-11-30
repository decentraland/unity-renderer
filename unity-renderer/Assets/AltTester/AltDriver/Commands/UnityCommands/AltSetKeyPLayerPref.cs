namespace Altom.AltDriver.Commands
{
    public class AltSetKeyPLayerPref : AltBaseCommand
    {
        AltSetKeyPlayerPrefParams cmdParams;
        public AltSetKeyPLayerPref(IDriverCommunication commHandler, string keyName, int intValue) : base(commHandler)
        {
            cmdParams = new AltSetKeyPlayerPrefParams(keyName, intValue);
        }
        public AltSetKeyPLayerPref(IDriverCommunication commHandler, string keyName, float floatValue) : base(commHandler)
        {
            cmdParams = new AltSetKeyPlayerPrefParams(keyName, floatValue);
        }
        public AltSetKeyPLayerPref(IDriverCommunication commHandler, string keyName, string stringValue) : base(commHandler)
        {
            cmdParams = new AltSetKeyPlayerPrefParams(keyName, stringValue);
        }
        public void Execute()
        {
            CommHandler.Send(cmdParams);
            var data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);
        }
    }
}