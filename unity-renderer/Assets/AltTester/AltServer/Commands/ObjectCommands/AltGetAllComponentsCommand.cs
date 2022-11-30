using System.Collections.Generic;
using Altom.AltDriver;
using Altom.AltDriver.Commands;
using Altom.AltTester.Logging;

namespace Altom.AltTester.Commands
{
    class AltGetAllComponentsCommand : AltCommand<AltGetAllComponentsParams, List<AltComponent>>
    {
        private static readonly NLog.Logger logger = ServerLogManager.Instance.GetCurrentClassLogger();

        public AltGetAllComponentsCommand(AltGetAllComponentsParams cmdParams) : base(cmdParams)
        {
        }

        public override List<AltComponent> Execute()
        {
            UnityEngine.GameObject altObject = AltRunner.GetGameObject(CommandParams.altObjectId);
            var listComponents = new List<AltComponent>();
            foreach (var component in altObject.GetComponents<UnityEngine.Component>())
            {
                try
                {
                    var a = component.GetType();
                    var componentName = a.FullName;
                    var assemblyName = a.Assembly.GetName().Name;
                    listComponents.Add(new AltComponent(componentName, assemblyName));
                }
                catch (System.NullReferenceException e)
                {
                    logger.Error(e);
                }
            }

            return listComponents;
        }
    }
}
