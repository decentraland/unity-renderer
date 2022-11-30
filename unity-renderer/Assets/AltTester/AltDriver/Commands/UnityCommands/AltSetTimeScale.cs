using Newtonsoft.Json;

namespace Altom.AltDriver.Commands
{
    public class AltSetTimeScale : AltBaseCommand
    {
        AltSetTimeScaleParams cmdParams;

        public AltSetTimeScale(IDriverCommunication commHandler, float timeScale) : base(commHandler)
        {
            cmdParams = new AltSetTimeScaleParams(timeScale);
        }
        public void Execute()
        {
            CommHandler.Send(cmdParams);
            var data = CommHandler.Recvall<string>(cmdParams);
            ValidateResponse("Ok", data);
        }
    }
}