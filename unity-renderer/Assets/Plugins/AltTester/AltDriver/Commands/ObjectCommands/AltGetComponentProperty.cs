namespace Altom.AltDriver.Commands
{
    public class AltGetComponentProperty<T> : AltBaseCommand
    {
        AltGetObjectComponentPropertyParams cmdParams;
        public AltGetComponentProperty(IDriverCommunication commHandler, string componentName, string propertyName, string assemblyName, int maxDepth, AltObject altObject) : base(commHandler)
        {
            cmdParams = new AltGetObjectComponentPropertyParams(altObject, componentName, propertyName, assemblyName, maxDepth);
        }
        public T Execute()
        {
            CommHandler.Send(cmdParams);
            return CommHandler.Recvall<T>(cmdParams);
        }
    }
}