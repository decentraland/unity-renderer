namespace Altom.AltDriver.Commands
{
    public class AltGetAllActiveCameras : AltBaseFindObjects
    {
        private readonly AltGetAllActiveCamerasParams cmdParams;
        public AltGetAllActiveCameras(IDriverCommunication commHandler) : base(commHandler)
        {

            cmdParams = new AltGetAllActiveCamerasParams();
        }
        public System.Collections.Generic.List<AltObject> Execute()
        {
            CommHandler.Send(cmdParams);
            return ReceiveListOfAltObjects(cmdParams);
        }
    }
}