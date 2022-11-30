using Altom.AltDriver.Logging;

namespace Altom.AltDriver.Commands
{
    public class AltSetServerLogging : AltBaseCommand
    {
        private readonly AltSetServerLoggingParams cmdParams;

        public AltSetServerLogging(IDriverCommunication commHandler, AltLogger logger, AltLogLevel logLevel) : base(commHandler)
        {
            this.cmdParams = new AltSetServerLoggingParams(logger, logLevel);
        }
        public void Execute()
        {
            this.CommHandler.Send(this.cmdParams);
            var data = this.CommHandler.Recvall<string>(this.cmdParams);
            ValidateResponse("Ok", data);
        }
    }
}
