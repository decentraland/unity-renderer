using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Altom.AltTester.Communication;

namespace Altom.AltTester.Commands
{
    public class AltGetScreenshotCommand : AltBaseScreenshotCommand<AltGetScreenshotParams, string>
    {
        public AltGetScreenshotCommand(ICommandHandler handler, AltGetScreenshotParams cmdParams) : base(handler, cmdParams)
        {
        }

        public override string Execute()
        {
            AltRunner._altRunner.StartCoroutine(SendTexturedScreenshotCoroutine(CommandParams.size.ToUnity(), CommandParams.quality));
            return "Ok";
        }
    }
}
