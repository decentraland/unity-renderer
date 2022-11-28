namespace Altom.AltDriver.Commands
{
    public class AltFindObject : AltBaseFindObjects
    {
        AltFindObjectParams cmdParams;

        public AltFindObject(IDriverCommunication commHandler, By by, string value, By cameraBy, string cameraValue, bool enabled) : base(commHandler)
        {
            cameraValue = SetPath(cameraBy, cameraValue);
            string path = SetPath(by, value);
            cmdParams = new AltFindObjectParams(path, cameraBy, cameraValue, enabled);
        }

        public AltObject Execute()
        {
            CommHandler.Send(cmdParams);
            var altTesterObject = CommHandler.Recvall<AltObject>(cmdParams);
            altTesterObject.CommHandler = CommHandler;
            return altTesterObject;
        }
    }
}