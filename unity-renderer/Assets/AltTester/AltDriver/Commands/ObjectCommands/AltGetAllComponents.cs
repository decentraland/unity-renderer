using System.Collections.Generic;
using Newtonsoft.Json;

namespace Altom.AltDriver.Commands
{
    public class AltGetAllComponents : AltBaseCommand
    {
        AltGetAllComponentsParams cmdParams;
        public AltGetAllComponents(IDriverCommunication commHandler, AltObject altObject) : base(commHandler)
        {
            cmdParams = new AltGetAllComponentsParams(altObject.id);
        }
        public List<AltComponent> Execute()
        {
            CommHandler.Send(cmdParams);
            return CommHandler.Recvall<List<AltComponent>>(cmdParams);

        }
    }
}