using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

public class ECS7ComponentsPlugin : IPlugin
{
    private PoolableComponentFactory poolableComponentFactory;

    public ECS7ComponentsPlugin()
    {
        poolableComponentFactory = Resources.Load<PoolableComponentFactory>("PoolableCoreComponentsFactory");
        IRuntimeComponentFactory factory = Environment.i.world.componentFactory; 
        
        // When we have clear how do we want to create components in the scenes, we should register here the components
    }
    
    public void Dispose()
    {
        
    }
    
}
