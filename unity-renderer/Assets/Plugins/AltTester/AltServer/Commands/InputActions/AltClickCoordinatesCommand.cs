using System;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Altom.AltTester.Communication;

namespace Altom.AltTester.Commands
{
    public class AltClickCoordinatesCommand : AltCommandWithWait<AltClickCoordinatesParams, string>
    {
        public AltClickCoordinatesCommand(ICommandHandler handler, AltClickCoordinatesParams cmdParams) : base(cmdParams, handler, cmdParams.wait)
        {
        }
        public override string Execute()
        {

            InputController.ClickCoordinates(CommandParams.coordinates.ToUnity(), CommandParams.count, CommandParams.interval, onFinish);
            return "Ok";
        }
    }
}
