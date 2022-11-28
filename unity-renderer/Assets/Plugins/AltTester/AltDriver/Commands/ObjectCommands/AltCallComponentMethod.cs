using System.Linq;
using Newtonsoft.Json;

namespace Altom.AltDriver.Commands
{
    public class AltCallComponentMethod<T> : AltBaseCommand
    {
        AltCallComponentMethodForObjectParams cmdParams;

        public AltCallComponentMethod(IDriverCommunication commHandler, string componentName, string methodName, object[] parameters, string[] typeOfParameters, string assembly, AltObject altObject) : base(commHandler)
        {
            cmdParams = new AltCallComponentMethodForObjectParams(altObject, componentName, methodName, parameters.Select(p => JsonConvert.SerializeObject(p, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Culture = System.Globalization.CultureInfo.InvariantCulture
            })).ToArray(), typeOfParameters, assembly);
        }
        public T Execute()
        {
            CommHandler.Send(cmdParams);
            T data = CommHandler.Recvall<T>(cmdParams);
            return data;
        }
    }
}