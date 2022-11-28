using System;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Altom.AltTester.Communication;

namespace Altom.AltTester.Commands
{
    class AltScrollCommand : AltCommandWithWait<AltScrollParams, string>
    {


        public AltScrollCommand(ICommandHandler handler, AltScrollParams cmdParams) : base(cmdParams, handler, cmdParams.wait)
        {
        }

        public override string Execute()
        {
            InputController.Scroll(CommandParams.speed, CommandParams.speedHorizontal, CommandParams.duration, onFinish);
            return "Ok";
        }
    }
}
