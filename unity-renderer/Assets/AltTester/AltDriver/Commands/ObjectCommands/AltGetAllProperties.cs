using System.Collections.Generic;

namespace Altom.AltDriver.Commands
{
    public class AltGetAllProperties : AltBaseCommand
    {
        AltGetAllPropertiesParams cmdParams;

        public AltGetAllProperties(IDriverCommunication commHandler, AltComponent altComponent, AltObject altObject, AltPropertiesSelections altPropertiesSelections = AltPropertiesSelections.ALLPROPERTIES) : base(commHandler)
        {
            cmdParams = new AltGetAllPropertiesParams(altObject.id, altComponent, altPropertiesSelections);

        }
        public List<AltProperty> Execute()
        {
            CommHandler.Send(cmdParams);
            return CommHandler.Recvall<List<AltProperty>>(cmdParams);
        }
    }
}