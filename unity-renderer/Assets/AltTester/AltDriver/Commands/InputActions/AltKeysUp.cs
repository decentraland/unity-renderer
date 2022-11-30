namespace Altom.AltDriver.Commands
{
    public class AltKeysUp : AltBaseCommand
    {
        AltKeysUpParams cmdParams;

        public AltKeysUp(IDriverCommunication commHandler, AltKeyCode[] keyCodes) : base(commHandler)
        {
            this.cmdParams = new AltKeysUpParams(keyCodes);
        }
        public void Execute()
        {
            CommHandler.Send(cmdParams);
            var data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);
        }
    }
}