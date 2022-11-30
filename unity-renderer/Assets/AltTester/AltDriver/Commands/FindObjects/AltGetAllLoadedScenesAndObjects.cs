using System.Collections.Generic;

namespace Altom.AltDriver.Commands
{
    public class AltGetAllLoadedScenesAndObjects : AltBaseFindObjects
    {

        private AltGetAllLoadedScenesAndObjectsParams cmdParams;
        public AltGetAllLoadedScenesAndObjects(IDriverCommunication commHandler, bool enabled) : base(commHandler)
        {
            cmdParams = new AltGetAllLoadedScenesAndObjectsParams("//*", By.NAME, "", enabled);
        }
        public List<AltObjectLight> Execute()
        {
            CommHandler.Send(cmdParams);
            return CommHandler.Recvall<List<AltObjectLight>>(cmdParams);

        }
    }
}