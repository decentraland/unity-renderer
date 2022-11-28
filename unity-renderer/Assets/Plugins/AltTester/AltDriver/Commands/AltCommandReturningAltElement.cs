using System.Collections.Generic;
using Newtonsoft.Json;

namespace Altom.AltDriver.Commands
{
    public class AltCommandReturningAltElement : AltBaseCommand
    {
        public AltCommandReturningAltElement(IDriverCommunication commHandler) : base(commHandler)
        {
        }

        protected AltObject ReceiveAltObject(CommandParams cmdParams)
        {
            var altElement = CommHandler.Recvall<AltObject>(cmdParams);
            if (altElement != null) altElement.CommHandler = CommHandler;

            return altElement;
        }
        protected List<AltObject> ReceiveListOfAltObjects(CommandParams cmdParams)
        {
            var altElements = CommHandler.Recvall<List<AltObject>>(cmdParams);

            foreach (var altElement in altElements)
            {
                altElement.CommHandler = CommHandler;
            }

            return altElements;
        }
    }
}