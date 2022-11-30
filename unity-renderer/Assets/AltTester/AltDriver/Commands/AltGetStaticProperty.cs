using System.Collections.Generic;
using Newtonsoft.Json;

namespace Altom.AltDriver.Commands
{
    public class AltGetStaticProperty<T> : AltBaseCommand
    {
        AltGetObjectComponentPropertyParams cmdParams;
        public AltGetStaticProperty(IDriverCommunication commHandler, string componentName, string propertyName, string assemblyName, int maxDepth) : base(commHandler)
        {
            cmdParams = new AltGetObjectComponentPropertyParams(null, componentName, propertyName, assemblyName, maxDepth);
        }
        public T Execute()
        {
            CommHandler.Send(cmdParams);
            T data = CommHandler.Recvall<T>(cmdParams);
            return data;
        }
    }
}