namespace Altom.AltDriver.Commands
{
    public class AltFindObjectsWhichContain : AltBaseFindObjects
    {
        AltFindObjectsParams cmdParams;

        public AltFindObjectsWhichContain(IDriverCommunication commHandler, By by, string value, By cameraBy, string cameraValue, bool enabled) : base(commHandler)
        {
            cmdParams = new AltFindObjectsParams(SetPathContains(by, value), cameraBy, SetPath(cameraBy, cameraValue), enabled);

        }
        public System.Collections.Generic.List<AltObject> Execute()
        {
            CommHandler.Send(cmdParams);
            return ReceiveListOfAltObjects(cmdParams);
        }
    }
}
