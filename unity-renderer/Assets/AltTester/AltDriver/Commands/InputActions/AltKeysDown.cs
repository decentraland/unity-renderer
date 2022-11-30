using System;

namespace Altom.AltDriver.Commands
{
    public class AltKeysDown : AltBaseCommand
    {
        AltKeysDownParams cmdParams;

        public AltKeysDown(IDriverCommunication commHandler, AltKeyCode[] keyCodes, float power) : base(commHandler)
        {
            this.cmdParams = new AltKeysDownParams(keyCodes, power);
        }
        public void Execute()
        {
            CommHandler.Send(cmdParams);
            var data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);
        }
    }
}