using System;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Altom.AltTester.Communication;
using UnityEngine;

namespace Altom.AltTester.Commands
{
    class AltPressKeyboardKeysCommand : AltCommandWithWait<AltPressKeyboardKeysParams, string>
    {
        public AltPressKeyboardKeysCommand(ICommandHandler handler, AltPressKeyboardKeysParams cmdParams) : base(cmdParams, handler, cmdParams.wait)
        {
        }

        public override string Execute()
        {
#if ALTTESTER
            var powerClamped = Mathf.Clamp01(CommandParams.power);
            foreach (var keyCode in CommandParams.keyCodes)
                InputController.PressKey((UnityEngine.KeyCode)keyCode, CommandParams.power, CommandParams.duration, onFinish);
            return "Ok";
#else
            throw new AltInputModuleException(AltErrors.errorInputModule);
#endif
        }
    }
}
