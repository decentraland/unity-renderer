using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSRuntime;

namespace ECSSystems.Helpers
{
    public static class ECSSystemsReferencesContainer
    {
        public static IReadOnlyList<IParcelScene> loadedScenes;
        public static IECSComponentWriter componentsWriter;
    }
}