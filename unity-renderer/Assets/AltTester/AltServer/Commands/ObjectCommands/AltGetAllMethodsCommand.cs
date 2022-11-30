using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Altom.AltDriver;
using Altom.AltDriver.Commands;

namespace Altom.AltTester.Commands
{
    class AltGetAllMethodsCommand : AltReflectionMethodsCommand<AltGetAllMethodsParams, List<string>>
    {
        public AltGetAllMethodsCommand(AltGetAllMethodsParams cmdParams) : base(cmdParams)
        {
        }

        public override List<string> Execute()
        {
            Type type = GetType(CommandParams.altComponent.componentName, CommandParams.altComponent.assemblyName);
            MethodInfo[] methodInfos = new MethodInfo[1];
            switch (CommandParams.methodSelection)
            {
                case AltMethodSelection.CLASSMETHODS:
                    methodInfos = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    break;
                case AltMethodSelection.INHERITEDMETHODS:
                    var allMethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    var classMethods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    methodInfos = allMethods.Except(classMethods).ToArray();
                    break;
                case AltMethodSelection.ALLMETHODS:
                    methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    break;
            }

            var listMethods = new List<string>();

            foreach (var methodInfo in methodInfos)
            {
                listMethods.Add(methodInfo.ToString());
            }
            return listMethods;
        }
    }
}
