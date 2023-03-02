using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

public static class DataStore_SceneBoundariesChecker_Extensions 
{
    public static void Add(this DataStore_SceneBoundariesChecker self,IDCLEntity entity, IOutOfSceneBoundariesHandler handler)
    {
        if (!self.componentsCheckSceneBoundaries.TryGetValue(entity.entityId, out HashSet<IOutOfSceneBoundariesHandler> handlersList))
            self.componentsCheckSceneBoundaries.Add(entity.entityId,new HashSet<IOutOfSceneBoundariesHandler>() { handler });
        else
            handlersList.Add(handler);
    }
    
    public static void Remove(this DataStore_SceneBoundariesChecker self,IDCLEntity entity, IOutOfSceneBoundariesHandler handler)
    {
        if (!self.componentsCheckSceneBoundaries.TryGetValue(entity.entityId, out HashSet<IOutOfSceneBoundariesHandler> handlersList))
            return;

        if (handlersList.Count <= 1)
            self.componentsCheckSceneBoundaries.Remove(entity.entityId);
        else
            handlersList.Remove(handler);
    }
}
