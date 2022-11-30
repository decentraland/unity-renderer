using System;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Altom.AltTester.Communication;

namespace Altom.AltTester.Commands
{
    class AltTiltCommand : AltCommandWithWait<AltTiltParams, string>
    {
        public AltTiltCommand(ICommandHandler handler, AltTiltParams cmdParams) : base(cmdParams, handler, cmdParams.wait)
        {
        }

        public override string Execute()
        {
            InputController.Tilt(CommandParams.acceleration.ToUnity(), CommandParams.duration, onFinish);
            return "Ok";
        }

    }
}
