namespace Altom.AltDriver.Commands
{
    public class AltClickElement : AltCommandReturningAltElement
    {
        AltClickElementParams cmdParams;

        public AltClickElement(IDriverCommunication commHandler, AltObject altObject, int count, float interval, bool wait) : base(commHandler)
        {
            cmdParams = new AltClickElementParams(
            altObject,
             count,
             interval,
             wait);
        }
        public AltObject Execute()
        {
            CommHandler.Send(cmdParams);
            var element = ReceiveAltObject(cmdParams);

            if (cmdParams.wait)
            {
                var data = CommHandler.Recvall<string>(cmdParams);
                ValidateResponse("Finished", data);
            }
            return element;
        }
    }
}