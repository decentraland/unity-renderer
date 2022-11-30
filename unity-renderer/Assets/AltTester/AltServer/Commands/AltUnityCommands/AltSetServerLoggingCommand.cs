using Altom.AltDriver.Commands;
using Altom.AltTester.Logging;

namespace Altom.AltTester.Commands
{
    public class AltSetServerLoggingCommand : AltCommand<AltSetServerLoggingParams, string>
    {
        public AltSetServerLoggingCommand(AltSetServerLoggingParams cmdParams) : base(cmdParams)
        {
        }

        public override string Execute()
        {
            ServerLogManager.SetMinLogLevel(CommandParams.logger, CommandParams.logLevel);
            return "Ok";
        }
    }
}
