using System;
using Altom.AltDriver;
using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    public class AltInvalidCommand : AltCommand<CommandParams, string>
    {
        private readonly Exception ex;


        public AltInvalidCommand(CommandParams cmdParams, Exception ex) : base(cmdParams ?? new CommandParams(AltErrors.errorInvalidCommand, null))
        {
            this.ex = ex;
        }
        public override string Execute()
        {
            throw new InvalidCommandException(ex);
        }
    }
}
