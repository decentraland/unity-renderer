namespace Altom.AltDriver.Commands
{
    public class AltPointerUpFromObject : AltCommandReturningAltElement
    {
        AltPointerUpFromObjectParams cmdParams;

        public AltPointerUpFromObject(IDriverCommunication commHandler, AltObject altObject) : base(commHandler)
        {
            this.cmdParams = new AltPointerUpFromObjectParams(altObject);
        }
        public AltObject Execute()
        {
            CommHandler.Send(cmdParams);
            return ReceiveAltObject(cmdParams);
        }
    }
}