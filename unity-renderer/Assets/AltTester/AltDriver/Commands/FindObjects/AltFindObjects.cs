namespace Altom.AltDriver.Commands
{
    public class AltFindObjects : AltBaseFindObjects
    {
        AltFindObjectsParams cmdParams;

        public AltFindObjects(IDriverCommunication commHandler, By by, string value, By cameraBy, string cameraValue, bool enabled) : base(commHandler)
        {
            cmdParams = new AltFindObjectsParams(SetPath(by, value), cameraBy, SetPath(cameraBy, cameraValue), enabled);
        }
        public System.Collections.Generic.List<AltObject> Execute()
        {
            CommHandler.Send(cmdParams);
            return ReceiveListOfAltObjects(cmdParams);
        }
    }
}