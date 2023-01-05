using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using System.Collections.Generic;

namespace DCL.ECSComponents
{
    public static class PointerEventsExtensions
    {
        public static IReadOnlyList<PBPointerEvents.Types.Entry> GetPointerEventsForEntity(this ECSComponent<PBPointerEvents> component,
            IParcelScene scene, IDCLEntity entity)
        {
            var componentData = component.Get(scene, entity);
            return componentData?.model.PointerEvents;
        }
    }
}
