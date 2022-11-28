using Altom.AltDriver.Commands;
using Altom.AltTester.Communication;

namespace Altom.AltTester.Commands
{
    class AltGetScreenshotPNGCommand : AltBaseScreenshotCommand<AltGetPNGScreenshotParams, string>
    {
        public AltGetScreenshotPNGCommand(ICommandHandler handler, AltGetPNGScreenshotParams cmdParams) : base(handler, cmdParams)
        {
        }

        public override string Execute()
        {
            AltRunner._altRunner.StartCoroutine(SendPNGScreenshotCoroutine());
            return "Ok";
        }
    }
}
