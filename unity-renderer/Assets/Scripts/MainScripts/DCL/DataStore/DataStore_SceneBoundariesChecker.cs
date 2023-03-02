using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;


namespace DCL
{
    public class DataStore_SceneBoundariesChecker
    {
        public BaseDictionary<long, HashSet<IOutOfSceneBoundariesHandler>> componentsCheckSceneBoundaries = new BaseDictionary<long, HashSet<IOutOfSceneBoundariesHandler>>();
    }
}