namespace Altom.AltDriver.Commands
{
    public class AltPointerExitObject : AltCommandReturningAltElement
    {
        AltPointerExitObjectParams cmdParams;

        public AltPointerExitObject(IDriverCommunication commHandler, AltObject altObject) : base(commHandler)
        {
            this.cmdParams = new AltPointerExitObjectParams(altObject);
        }
        public AltObject Execute()
        {
            CommHandler.Send(cmdParams);
            return ReceiveAltObject(cmdParams);
        }
    }
}