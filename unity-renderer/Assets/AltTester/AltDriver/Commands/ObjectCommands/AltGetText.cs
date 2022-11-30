namespace Altom.AltDriver.Commands
{
    public class AltGetText : AltBaseCommand
    {
        readonly AltGetTextParams cmdParams;

        public AltGetText(IDriverCommunication commHandler, AltObject altObject) : base(commHandler)
        {
            cmdParams = new AltGetTextParams(altObject);
        }

        public string Execute()
        {
            CommHandler.Send(cmdParams);
            return CommHandler.Recvall<string>(cmdParams);
        }
    }
}