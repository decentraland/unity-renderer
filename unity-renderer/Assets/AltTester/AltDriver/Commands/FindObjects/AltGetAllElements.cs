namespace Altom.AltDriver.Commands
{
    public class AltGetAllElements : AltBaseFindObjects
    {
        AltFindObjectsParams cmdParams;

        public AltGetAllElements(IDriverCommunication commHandler, By cameraBy, string cameraValue, bool enabled) : base(commHandler)
        {
            cmdParams = new AltFindObjectsParams("//*", cameraBy, SetPath(cameraBy, cameraValue), enabled);
        }
        public System.Collections.Generic.List<AltObject> Execute()
        {
            CommHandler.Send(cmdParams);
            return ReceiveListOfAltObjects(cmdParams);
        }
    }
}