namespace Altom.AltDriver.Commands
{
    public class AltPointerEnterObject : AltCommandReturningAltElement
    {
        AltPointerEnterObjectParams cmdParams;
        public AltPointerEnterObject(IDriverCommunication commHandler, AltObject altObject) : base(commHandler)
        {
            cmdParams = new AltPointerEnterObjectParams(altObject);
        }
        public AltObject Execute()
        {
            CommHandler.Send(cmdParams);
            return ReceiveAltObject(cmdParams);
        }
    }
}