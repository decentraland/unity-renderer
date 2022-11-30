using System;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Altom.AltTester.Communication;

namespace Altom.AltTester.Commands
{
    public class AltTapCoordinatesCommand : AltCommandWithWait<AltTapCoordinatesParams, string>
    {
        public AltTapCoordinatesCommand(ICommandHandler handler, AltTapCoordinatesParams cmdParams) : base(cmdParams, handler, cmdParams.wait)
        {
        }

        public override string Execute()
        {
            InputController.TapCoordinates(CommandParams.coordinates.ToUnity(), CommandParams.count, CommandParams.interval, onFinish);
            return "Ok";
        }
    }
}