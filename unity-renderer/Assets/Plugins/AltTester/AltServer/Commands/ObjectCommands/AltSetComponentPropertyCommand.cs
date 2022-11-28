using Altom.AltDriver.Commands;


namespace Altom.AltTester.Commands
{
    class AltSetComponentPropertyCommand : AltReflectionMethodsCommand<AltSetObjectComponentPropertyParams, string>
    {
        public AltSetComponentPropertyCommand(AltSetObjectComponentPropertyParams cmdParams) : base(cmdParams)
        {
        }

        public override string Execute()
        {
            System.Type type = GetType(CommandParams.component, CommandParams.assembly);
            string response = SetValueForMember(CommandParams.altObject, CommandParams.property.Split('.'), type, CommandParams.value);

            return response;
        }
    }
}
