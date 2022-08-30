using System.Collections.Generic;
using DCL.ECSRuntime;
using DCLPlugins.ECSComponents.Events;

namespace DCL.ECS7
{
    public interface IECSContext
    {
        SystemsContext systemsContext { get; }
    }
    
    public readonly struct ECSContext : IECSContext
    {
        public readonly SystemsContext systemsContext { get; }

        public ECSContext(IECSComponentWriter componentWriter,
            IInternalECSComponents internalEcsComponents,
            ECSComponentsManager componentsManager)
        {
            systemsContext = new SystemsContext(componentWriter, internalEcsComponents, new ComponentGroups(componentsManager), new Queue<PointerEvent>());
        }
    }
}