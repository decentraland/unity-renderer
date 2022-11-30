using System.Collections.Generic;

namespace Altom.AltDriver.Commands
{
    public class AltGetAllElementsLight : AltBaseFindObjects
    {
        AltFindObjectsLightParams cmdParams;
        public AltGetAllElementsLight(IDriverCommunication commHandler, By cameraBy, string cameraValue, bool enabled) : base(commHandler)
        {
            cmdParams = new AltFindObjectsLightParams("//*", cameraBy, SetPath(cameraBy, cameraValue), enabled);
        }
        public List<AltObjectLight> Execute()
        {
            CommHandler.Send(cmdParams);

            return CommHandler.Recvall<List<AltObjectLight>>(cmdParams);
        }
    }
}