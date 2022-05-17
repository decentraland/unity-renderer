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
        if (self.componentsCheckSceneBoundaries.ContainsKey(entity))
            self.componentsCheckSceneBoundaries[entity].Add(handler);
        else
            self.componentsCheckSceneBoundaries.Add(entity,new List<IOutOfSceneBoundariesHandler>() { handler });
    }
    
    public static void Remove(this DataStore_SceneBoundariesChecker self,IDCLEntity entity, IOutOfSceneBoundariesHandler handler)
    {
        if (!self.componentsCheckSceneBoundaries.ContainsKey(entity))
            return;

        if (self.componentsCheckSceneBoundaries[entity].Count <= 1)
            self.componentsCheckSceneBoundaries.Remove(entity);
        else
            self.componentsCheckSceneBoundaries[entity].Remove(handler);
    }
}
